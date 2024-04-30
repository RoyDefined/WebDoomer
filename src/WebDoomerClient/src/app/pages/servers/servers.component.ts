import { AfterContentInit, AfterViewChecked, AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { CdkVirtualScrollViewport, ScrollingModule } from '@angular/cdk/scrolling';
import { ServersApiService } from '../../services/api/servers-api.service';
import { CommonModule } from '@angular/common';
import { ServersStore } from '../../stores/servers/servers.store';
import { ListedServerComponent } from '../../components/listed-server/listed-server.component';
import { Server } from '../../models/server';
import { ListRange } from '@angular/cdk/collections';
import { Subject, Subscription, debounceTime, distinctUntilChanged, map, tap, withLatestFrom } from 'rxjs';
import { ListedServerSkeletonComponent } from '../../components/listed-server-skeleton/listed-server-skeleton.component';
import { ServerSidebarComponent } from '../../components/server-sidebar/server-sidebar.component';
import { ModalService } from '../../services/modal/modal.service';
import { LearnMoreSchemeComponent } from '../../components/learn-more-scheme/learn-more-scheme.component';
import { ConfigureSchemeComponent } from '../../components/configure-scheme/configure-scheme.component';
import { ClientSettingsStore } from '../../stores/clientsettings/client-settings.store';
import { isMobile } from '../../utils/isMobile';
import { isWindows } from '../../utils/isWindows';
import { z } from 'zod';
import { clientSettingsSchema } from '../../stores/clientsettings/client-settings-schema';
import { MediaQuerySize } from '../../utils/media-query-size';
import { HeaderLeftComponent } from '../../services/header-ref/components/header-left.component';
import { HeaderRightComponent } from '../../services/header-ref/components/header-right.component';
import { HeaderBottomComponent } from '../../services/header-ref/components/header-bottom.component';
import { ServerHubStore } from '../../stores/signalr/server-hub.store';

@Component({
    standalone: true,
    templateUrl: './servers.component.html',
    host: {
        class: 'flex flex-col overflow-auto grow',
    },
    imports: [
        CommonModule,
        ScrollingModule,
        ListedServerComponent,
        ListedServerSkeletonComponent,
        ServerSidebarComponent,
        HeaderLeftComponent,
        HeaderRightComponent,
        HeaderBottomComponent,
    ],
    providers: [ModalService],
})
export class ServersComponent implements OnInit, AfterViewInit {
    @ViewChild(CdkVirtualScrollViewport) virtualScrollViewport!: CdkVirtualScrollViewport;

    /** An observable that points to the view model of the server store. */
    public readonly vm$ = this._serversStore.vm$;

    /** The server that is currently being focused for the sidebar.*/
    public selectedServer: Server | null = null;

    /** Represents the base number of additional servers that is added to these requests.
     * This does not represent the actual number of servers being fetched.
     * The actual number is calculated after.
     */
    private readonly _serverAdditionalFetchAmount = 50;

    /** Represents the minimum number of servers that must be fetched in order to trigger fetching.
     * A number below this amount will not trigger fetching.
     * This value is ignored if the start/end of a collection is reached, as no more servers come after.
     */
    private readonly _serverMinimumFetchAmount = 30;

    /** Specifies the first rendered index the last time the virtual scroll was rendered.
     * This value is used to determine the scroll direction in order to load more servers better.
     */
    private _firstIndexOnLastRender = 0;

    /** The subscription to the virtual scroll viewport that handles the virtual list.
     * Can be unsubscribed in the event of an error.
     */
    private _virtualScrollViewportSubscription?: Subscription;

    /** Indicates the search box is enabled. */
    public searchEnabled = false;

    /** The subject to handle search input changes */
    private _searchInputChange = new Subject<string | null>();

    public readonly settings$ = this._clientSettingsStore.settings$;

    /** Indicates at what media query size server rows are expected to be fully expanded. */
    public get expandListMediaQuerySize(): MediaQuerySize {
        return this.selectedServer ? 'xl' : 'md';
    }

    public get isMobile() {
        return isMobile();
    }

    public get isWindows() {
        return isWindows();
    }

    public get serverCount$() {
        return this.vm$.pipe(map((vm) => vm.servers.length));
    }

    constructor(
        private readonly _serversStore: ServersStore,
        private readonly _clientSettingsStore: ClientSettingsStore,
        private readonly _serverHubStore: ServerHubStore,
    ) {
        // Subscribe to search input changes and fetch the new id list based on the search query.
        this._searchInputChange
            .pipe(
                debounceTime(400),
                map((value) => value || ''),
                distinctUntilChanged(),
            )
            .subscribe((value) => {
                this._serversStore.getServerIdsWithSearchString(value);
            });
    }

    ngOnInit() {
        // Handle any errors coming from the store.
        this._serversStore.vm$.subscribe((vm) => {
            if (!vm.error) {
                return;
            }

            this._virtualScrollViewportSubscription?.unsubscribe();

            console.warn('There was an issue with one or more servers during fetching.');
            console.error(vm.error);
        });

        this._serversStore.getServerIds();

        // Handle signalR signal to refresh the server list.
        // TODO: Handle errors and disposing of subscription on error.
        this._serverHubStore.onRefreshServers.subscribe(() => {
            console.log('Server list refresh triggered.');
            this._serversStore.getServerIds();
        });
    }

    ngAfterViewInit() {
        this._virtualScrollViewportSubscription = this.virtualScrollViewport.renderedRangeStream.pipe(withLatestFrom(this.vm$)).subscribe((args) => {
            let range = args[0];
            const vm = args[1];
            const servers = vm.servers;

            if (vm.servers.length == 0) {
                return;
            }

            // Determine direction
            const firstIndex = range.start;
            const direction = firstIndex < this._firstIndexOnLastRender ? 'up' : 'down';
            this._firstIndexOnLastRender = firstIndex;

            // The directions implement sort of the same system.
            // For readability they have been split.
            // Determine what index to start, and what index to end.
            // One index is based on when the first fetchable index can be found.
            // The other is the last index that might be fetched.
            // After that a take is determined. If this take is too low the fetch won't happen.
            if (direction === 'down') {
                const startIndex = servers.findIndex((server, index, _) => index >= range.start && !server.fetching && server.state === 'id');
                if (startIndex == -1) {
                    return;
                }

                const endIndex = Math.min(range.end + this._serverAdditionalFetchAmount, servers.length);
                var reachedEnd = endIndex == servers.length;

                range = { start: startIndex, end: endIndex };
                var take = range.end - range.start;
            } else {
                const endIndex = servers.findLastIndex((server, index, _) => index <= range.end && !server.fetching && server.state === 'id');
                if (endIndex == -1) {
                    return;
                }

                const startIndex = Math.max(range.start - this._serverAdditionalFetchAmount, 0);
                var reachedEnd = startIndex == 0;

                range = { start: startIndex, end: endIndex + 1 };
                var take = range.end - range.start;
            }

            // Take must be a minimum unless we reached the end of the collection.
            if (!reachedEnd && take < this._serverMinimumFetchAmount) {
                return;
            }

            //console.log('Final fetch range', range);
            //console.log('Take', take);

            // The servers being fetched should indicate they are being fetched.
            for (const server of servers.slice(range.start, range.end)) {
                server.fetching = true;
            }

            this._serversStore.updateListedServersByRange(range);
        });
    }

    public trackByItemId(index: number, item: Server) {
        //console.log(index);
        return item.id;
    }

    public onServerClicked(server: Server) {
        if (this.selectedServer === server) {
            this.selectedServer = null;
            return;
        }

        this.selectedServer = server;
        this._serversStore.updateDetailedServer(server);
    }

    public onSidebarCollapse() {
        this.selectedServer = null;
    }

    public toggleSearchInput() {
        this.searchEnabled = !this.searchEnabled;
        if (!this.searchEnabled) {
            this._searchInputChange.next('');
        }
    }

    public onSearchInputChange(event: Event) {
        const value = (event.target as HTMLInputElement).value;
        this._searchInputChange.next(value);
    }
}

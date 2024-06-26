import { AfterViewInit, Component, Inject, OnInit, ViewChild } from '@angular/core';
import { CdkVirtualScrollViewport, ScrollingModule } from '@angular/cdk/scrolling';
import { CommonModule, DOCUMENT } from '@angular/common';
import { ServersStore } from '../../stores/servers/servers.store';
import { ListedServerComponent } from '../../components/listed-server/listed-server.component';
import { Server } from '../../models/server';
import { Subject, Subscription, debounceTime, distinctUntilChanged, map, tap, withLatestFrom } from 'rxjs';
import { ListedServerSkeletonComponent } from '../../components/listed-server-skeleton/listed-server-skeleton.component';
import { ServerSidebarComponent } from '../../components/server-sidebar/server-sidebar.component';
import { ModalService } from '../../services/modal/modal.service';
import { ClientSettingsStore } from '../../stores/clientsettings/client-settings.store';
import { isMobile } from '../../utils/isMobile';
import { isWindows } from '../../utils/isWindows';
import { MediaQuerySize } from '../../utils/media-query-size';
import { HeaderLeftComponent } from '../../services/header-ref/components/header-left.component';
import { HeaderRightComponent } from '../../services/header-ref/components/header-right.component';
import { HeaderBottomComponent } from '../../services/header-ref/components/header-bottom.component';
import { ServerHubStore } from '../../stores/signalr/server-hub.store';
import { PingStore } from '../../stores/ping/ping.store';
import { AppSettingsStore } from '../../stores/appsettings/app-settings.store';
import { ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CopyToClipboardDirective } from '../../directives/copy-to-clipboard-directive';

@Component({
    standalone: true,
    templateUrl: './servers.component.html',
    host: {
        class: 'flex flex-col overflow-auto grow',
    },
    imports: [
        CommonModule,
        FormsModule,
        ScrollingModule,
        ListedServerComponent,
        ListedServerSkeletonComponent,
        ServerSidebarComponent,
        HeaderLeftComponent,
        HeaderRightComponent,
        HeaderBottomComponent,
        CopyToClipboardDirective,
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

    /** The subscription to the signalR signal that indicates the server list should be refreshed.
     * Can be unsubscribed in the event of an error.
     */
    private _onRefreshServersSubscription?: Subscription;

    /** Indicates the search box is enabled. */
    public searchEnabled = false;

    /** Represents the permalink string that is generated using the search input and copied to the user. */
    public searchInputLink = '';

    /** The subject to handle search input changes */
    private _searchInputChange = new Subject<string | null>();

    public readonly settings$ = this._clientSettingsStore.settings$;

    /** Indicates at what media query size server rows are expected to be fully expanded. */
    public get expandListMediaQuerySize(): MediaQuerySize {
        return this.selectedServer ? 'xl' : 'md';
    }

    /** Represents the current search input. */
    private _searchInput = '';

    public get searchInput() {
        return this._searchInput;
    }

    public set searchInput(searchInput: string) {
        this._searchInput = searchInput;
        const search = encodeURIComponent(searchInput);
        this.searchInputLink = this._document.location.origin + this._document.location.pathname + '?search=' + search;
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
        private readonly _appSettingsStore: AppSettingsStore,
        private readonly _serversStore: ServersStore,
        private readonly _clientSettingsStore: ClientSettingsStore,
        private readonly _serverHubStore: ServerHubStore,
        private readonly _pingStore: PingStore,
        private readonly _activatedRoute: ActivatedRoute,
        @Inject(DOCUMENT) private readonly _document: Document,
    ) {
        // Subscribe to search input changes and fetch the new id list based on the search query.
        this._searchInputChange
            .pipe(
                map((value) => value || ''),
                distinctUntilChanged(),
                debounceTime(400),
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

            this.onSubscriptionError(vm.error);
        });

        // Handle any errors coming from the SignalR connection.
        this._serverHubStore.vm$.subscribe((vm) => {
            if (!vm.error) {
                return;
            }

            this.onSubscriptionError(vm.error);
        });

        // Handle incoming new servers.
        this._serversStore.servers$.subscribe((servers) => {
            // If a single server is returned and a search was issued, this server should be focused in the sidebar.
            if (servers.length != 1) {
                return;
            }

            const server = servers[0];

            if (!this.searchInput) {
                return;
            }

            if (this.selectedServer === server) {
                return;
            }

            this.selectedServer = server;
            this._serversStore.updateDetailedServer(server);
        });

        // Handle signalR signal to refresh the server list.
        this._onRefreshServersSubscription = this._serverHubStore.onRefreshServers.subscribe(() => {
            console.log('Server list refresh triggered.');
            this.selectedServer = null;
            this._serversStore.getServerIds();
            this._pingStore.getPing(this._appSettingsStore.settings.pingProtocol);
        });

        this._activatedRoute.queryParams.subscribe((params) => {
            // Request contains a search query parameter. Fetch using a search instead.
            const search = params['search'];
            if (search) {
                this.searchEnabled = true;
                this.searchInput = search;
                this._serversStore.getServerIdsWithSearchString(search);
                return;
            }
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
            this.searchInput = '';
        }
    }

    public onSearchInputChange(event: Event) {
        const value = (event.target as HTMLInputElement).value;
        this._searchInputChange.next(value);
    }

    private onSubscriptionError(error: Error) {
        this._virtualScrollViewportSubscription?.unsubscribe();
        this._onRefreshServersSubscription?.unsubscribe();

        console.warn('There was an issue with one of the stores.');
        console.error(error);
    }
}

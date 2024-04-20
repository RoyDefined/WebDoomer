import { AfterContentInit, AfterViewChecked, AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { CdkVirtualScrollViewport, ScrollingModule } from '@angular/cdk/scrolling';
import { ServersApiService } from '../../services/api/servers-api.service';
import { CommonModule } from '@angular/common';
import { ServersStore } from '../../stores/servers/servers.store';
import { ListedServerComponent } from '../../components/listed-server/listed-server.component';
import { Server } from '../../models/server';
import { ListRange } from '@angular/cdk/collections';
import { Subscription, map, tap } from 'rxjs';
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

    /** If `true`, hide the header informing the user to add an association to the specified url. */
    public hideRegistryTip = false;

    /** If `true`, hide the header informing the user to add personal configuration.
     * This variable might be turned on by default should the user already have valid configuration.
     */
    public hidePersonalConfigurationTip = false;

    /** If `true`, hide the header informing the user joining is only supported on Windows.
     */
    public hideWindowsOnlyTip = false;

    /** Specifies the first rendered index the last time the virtual scroll was rendered.
     * This value is used to determine the scroll direction in order to load more servers better.
     */
    private _firstIndexOnLastRender = 0;

    /** The subscription to the virtual scroll viewport that handles the virtual list.
     * Can be unsubscribed in the event of an error.
     */
    private _virtualScrollViewportSubscription?: Subscription;

    // Indicates the search box is enabled.
    public searchEnabled = false;

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

    public get shouldShowRegistryTip() {
        return this.isWindows && !this.isMobile && !this.hideRegistryTip;
    }

    public get shouldShowConfigurationTip$() {
        return this.settings$.pipe(
            map((settings) => this.isWindows && !this.isMobile && !this.hasConfiguredSomething(settings) && !this.hidePersonalConfigurationTip),
        );
    }

    public get shouldShowWindowsOnlyTip() {
        return !this.isWindows && !this.isMobile && !this.hideWindowsOnlyTip;
    }

    public get serverCount$() {
        return this.vm$.pipe(map((vm) => vm.servers.length));
    }

    constructor(
        private readonly _serversStore: ServersStore,
        private readonly _clientSettingsStore: ClientSettingsStore,
        private readonly _modalService: ModalService,
    ) {}

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
    }

    ngAfterViewInit() {
        this._virtualScrollViewportSubscription = this.virtualScrollViewport.renderedRangeStream.subscribe((range) => this.onVirtualListChange(range));
    }

    public trackByItemId(index: number, item: Server) {
        //console.log(index);
        return item.id;
    }

    public onVirtualListChange(range: ListRange) {
        // Determine direction
        const firstIndex = range.start;
        const direction = firstIndex < this._firstIndexOnLastRender ? 'up' : 'down';
        this._firstIndexOnLastRender = firstIndex;

        this._serversStore.updateListedServersByRangeAndDirection({ range, direction });
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

    public openLearnMoreScheme() {
        this._modalService.openModal(LearnMoreSchemeComponent);
    }

    public openConfigureScheme() {
        this._modalService.openModal(ConfigureSchemeComponent);
    }

    public hasConfiguredSomething(settings: z.infer<typeof clientSettingsSchema>) {
        return (
            !!settings.doomseekerLocation ||
            !!settings.iwadsLocation ||
            !!settings.pwadsLocation ||
            !!settings.qZandronumLocation ||
            !!settings.zandronumLocation
        );
    }
}

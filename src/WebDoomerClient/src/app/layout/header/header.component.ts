import { CommonModule, NgOptimizedImage } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ConfigureSchemeComponent } from '../../components/configure-scheme/configure-scheme.component';
import { ModalService } from '../../services/modal/modal.service';
import { ClientSettingsStore } from '../../stores/clientsettings/client-settings.store';
import { z } from 'zod';
import { darkModeTypeSchema } from '../../stores/clientsettings/darkModeType';
import { isMobile } from '../../utils/isMobile';
import { isWindows } from '../../utils/isWindows';
import { HeaderRefService } from '../../services/header-ref/header-ref.service';
import { clientSettingsSchema } from '../../stores/clientsettings/client-settings-schema';
import { LearnMoreSchemeComponent } from '../../components/learn-more-scheme/learn-more-scheme.component';
import { map } from 'rxjs';
import { ToolTipDirective } from '../../directives/tooltip-directive';
import { PingStore } from '../../stores/ping/ping.store';

@Component({
    standalone: true,
    selector: 'app-header',
    templateUrl: './header.component.html',
    imports: [CommonModule, RouterModule, NgOptimizedImage, ToolTipDirective],
    providers: [ModalService],
})
export class HeaderComponent implements OnInit {
    public readonly settings$ = this._clientSettingsStore.settings$;

    public readonly vm$ = this._pingStore.vm$;

    /** If `true`, hide the header informing the user to add an association to the specified url. */
    public hideRegistryTip = false;

    /** If `true`, hide the header informing the user to add personal configuration.
     * This variable might be turned on by default should the user already have valid configuration.
     */
    public hidePersonalConfigurationTip = false;

    /** If `true`, hide the header informing the user joining is only supported on Windows. */
    public hideWindowsOnlyTip = false;

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

    public get isMobile() {
        return isMobile();
    }

    public get isWindows() {
        return isWindows();
    }

    public get headerLeftRef() {
        return this._headerRefService.leftRef;
    }

    public get headerRightRef() {
        return this._headerRefService.rightRef;
    }

    public get headerBottomRef() {
        return this._headerRefService.bottomRef;
    }

    constructor(
        private readonly _modalService: ModalService,
        private readonly _clientSettingsStore: ClientSettingsStore,
        private readonly _headerRefService: HeaderRefService,
        private readonly _pingStore: PingStore,
    ) {}

    public ngOnInit() {
        this._pingStore.getPing();
    }

    public getPingState(ping: number | null) {
        return !ping ? 'None' : ping < 400 ? 'Healthy' : ping < 4000 ? 'Degraded' : 'Unhealthy';
    }

    public getPingMessage(ping: number | null) {
        const state = this.getPingState(ping);
        const message =
            state === 'Healthy'
                ? 'Healthy connection'
                : state === 'Degraded'
                  ? 'Degraded connection'
                  : state === 'Unhealthy'
                    ? 'Unhealthy connection'
                    : 'No connection';

        if (!ping) {
            return message;
        }

        return message + ` (${ping}ms)`;
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

    /**
     * Toggles dark mode to the next option.
     */
    public toggleDarkMode(type: z.infer<typeof darkModeTypeSchema>) {
        const nextType = type === 'System' ? 'Light' : type === 'Light' ? 'Dark' : 'System';
        this._clientSettingsStore.setDarkmodeType(nextType);
    }
}

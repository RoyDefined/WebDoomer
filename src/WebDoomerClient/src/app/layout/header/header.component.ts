import { CommonModule, NgOptimizedImage } from '@angular/common';
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ConfigureSchemeComponent } from '../../components/configure-scheme/configure-scheme.component';
import { ModalService } from '../../services/modal/modal.service';
import { ClientSettingsStore } from '../../stores/clientsettings/client-settings.store';
import { z } from 'zod';
import { darkModeTypeSchema } from '../../stores/clientsettings/darkModeType';
import { isMobile } from '../../utils/isMobile';
import { isWindows } from '../../utils/isWindows';
import { HeaderRefService } from '../../services/header-ref/header-ref.service';

@Component({
    standalone: true,
    selector: 'app-header',
    templateUrl: './header.component.html',
    imports: [CommonModule, RouterModule, NgOptimizedImage],
    providers: [ModalService],
})
export class HeaderComponent {
    public readonly settings$ = this._clientSettingsStore.settings$;

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

    constructor(
        private readonly _modalService: ModalService,
        private readonly _clientSettingsStore: ClientSettingsStore,
        private readonly _headerRefService: HeaderRefService,
    ) {}

    public openConfigureScheme() {
        this._modalService.openModal(ConfigureSchemeComponent);
    }

    /**
     * Toggles dark mode to the next option.
     */
    public toggleDarkMode(type: z.infer<typeof darkModeTypeSchema>) {
        const nextType = type === 'System' ? 'Light' : type === 'Light' ? 'Dark' : 'System';
        this._clientSettingsStore.setDarkmodeType(nextType);
    }
}

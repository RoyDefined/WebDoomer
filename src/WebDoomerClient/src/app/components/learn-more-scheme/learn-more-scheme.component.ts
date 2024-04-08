import { Component, OnInit } from '@angular/core';
import { Modal } from '../../services/modal/models/modal';
import { TabsComponent } from '../tabs/tabs.component';
import { TabComponent } from '../tabs/tab.component';
import { ClientSettingsStore } from '../../stores/clientsettings/client-settings.store';
import { CommonModule } from '@angular/common';
import { ModalService } from '../../services/modal/modal.service';
import { ConfigureSchemeComponent } from '../configure-scheme/configure-scheme.component';
import { RegisteryFileStore } from '../../stores/registeryfile/registery-file.store';

@Component({
    standalone: true,
    selector: 'app-learn-more-scheme',
    templateUrl: './learn-more-scheme.component.html',
    imports: [CommonModule, TabsComponent, TabComponent],
    providers: [ModalService],
})
export class LearnMoreSchemeComponent extends Modal implements OnInit {
    public readonly settings$ = this._clientSettingsStore.settings$;
    public readonly vm$ = this._registeryFileStore.vm$;
    constructor(
        private readonly _modalService: ModalService,
        private readonly _clientSettingsStore: ClientSettingsStore,
        private readonly _registeryFileStore: RegisteryFileStore,
    ) {
        super();
    }

    ngOnInit() {
        this._registeryFileStore.fetchRegistryFile();
    }

    public openConfigureScheme() {
        this.close();
        this._modalService.openModal(ConfigureSchemeComponent);
    }

    public downloadString(data: string) {
        const blob = new Blob([data], { type: 'application/octet-stream' });
        const url = window.URL.createObjectURL(blob);
        const anchor: HTMLAnchorElement = document.createElement('a') as HTMLAnchorElement;

        anchor.href = url;
        anchor.download = 'Doomseeker-Protocol.reg';
        document.body.appendChild(anchor);
        anchor.click();

        document.body.removeChild(anchor);
        URL.revokeObjectURL(url);
    }
}

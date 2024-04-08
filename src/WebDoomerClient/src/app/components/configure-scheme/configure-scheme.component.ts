import { Component } from '@angular/core';
import { Modal } from '../../services/modal/models/modal';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ClientSettingsStore } from '../../stores/clientsettings/client-settings.store';
import { RegisteryFileStore } from '../../stores/registeryfile/registery-file.store';

@Component({
    standalone: true,
    selector: 'app-configure-scheme',
    templateUrl: './configure-scheme.component.html',
    imports: [CommonModule, ReactiveFormsModule],
})
export class ConfigureSchemeComponent extends Modal {
    private readonly _filePathValidationRegex = /^[a-z]:((\\|\/)[a-z0-9\s_@\-^!.,#$%&+={}()\[\]]+)+\.exe$/i;
    private readonly _folderPathValidationRegex = /^[a-z]:((\\|\/)[a-z0-9\s_@\-^!.,#$%&+={}()\[\]]+)+(\\|\/)$/i;

    private _formGroup?: FormGroup;

    public get formGroup() {
        return this._formGroup;
    }

    public get zandronumLocationControl() {
        return this._formGroup!.controls['zandronumLocation'];
    }

    public get zandronumLocationErrors() {
        return this._formGroup!.controls['zandronumLocation'].errors != null;
    }

    public get zandronumLocationExecutablePathError() {
        return this._formGroup!.controls['zandronumLocation'].errors?.['executablepatherror'] == undefined;
    }

    public get qZandronumLocationControl() {
        return this._formGroup!.controls['qZandronumLocation'];
    }

    public get qZandronumLocationErrors() {
        return this._formGroup!.controls['qZandronumLocation'].errors != null;
    }

    public get qZandronumLocationExecutablePathError() {
        return this._formGroup!.controls['qZandronumLocation'].errors?.['executablepatherror'] == undefined;
    }

    public get doomseekerLocationControl() {
        return this._formGroup!.controls['doomseekerLocation'];
    }

    public get doomseekerLocationErrors() {
        return this._formGroup!.controls['doomseekerLocation'].errors != null;
    }

    public get doomseekerLocationExecutablePathError() {
        return this._formGroup!.controls['doomseekerLocation'].errors?.['executablepatherror'] == undefined;
    }

    public get pwadsLocationControl() {
        return this._formGroup!.controls['pwadsLocation'];
    }

    public get pwadsLocationErrors() {
        return this._formGroup!.controls['pwadsLocation'].errors != null;
    }

    public get pwadsLocationFolderPathError() {
        return this._formGroup!.controls['pwadsLocation'].errors?.['folderpatherror'] == undefined;
    }

    public get iwadsLocationControl() {
        return this._formGroup!.controls['iwadsLocation'];
    }

    public get iwadsLocationErrors() {
        return this._formGroup!.controls['iwadsLocation'].errors != null;
    }

    public get iwadsLocationFolderPathError() {
        return this._formGroup!.controls['iwadsLocation'].errors?.['folderpatherror'] == undefined;
    }

    constructor(
        private readonly _formbuilder: FormBuilder,
        private readonly _clientSettingsStore: ClientSettingsStore,
        private readonly _registeryFileStore: RegisteryFileStore,
    ) {
        super();

        this.validateExecutable = this.validateExecutable.bind(this);
        this.validateFolder = this.validateFolder.bind(this);
        this._clientSettingsStore.settings$.subscribe((settings) => {
            this._formGroup = this._formbuilder.group({
                zandronumLocation: [settings.zandronumLocation, this.validateExecutable],
                qZandronumLocation: [settings.qZandronumLocation, this.validateExecutable],
                doomseekerLocation: [settings.doomseekerLocation, this.validateExecutable],
                pwadsLocation: [settings.pwadsLocation, this.validateFolder],
                iwadsLocation: [settings.iwadsLocation, this.validateFolder],
            });
        });
    }

    private validateExecutable(control: AbstractControl) {
        if (typeof control.value !== 'string') {
            return null;
        }

        if (!control.value) {
            return null;
        }

        if (!this._filePathValidationRegex.test(control.value)) {
            return { executablepatherror: true };
        }

        return null;
    }

    private validateFolder(control: AbstractControl) {
        if (typeof control.value !== 'string') {
            return null;
        }

        if (!control.value) {
            return null;
        }

        if (!this._folderPathValidationRegex.test(control.value)) {
            return { folderpatherror: true };
        }

        return null;
    }

    public onFormClear() {
        this._formGroup!.reset();
    }

    public onFormSubmit() {
        this._clientSettingsStore.setFormSettings(this._formGroup!.value);
        this._registeryFileStore.updateRegisteryFileContent();
        this.close();
    }
}

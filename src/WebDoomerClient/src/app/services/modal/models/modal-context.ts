import { ComponentRef } from '@angular/core';
import { ModalContainerComponent } from '../components/modal-container-component';
import { Modal } from './modal';
import { Subject } from 'rxjs';

export class ModalContext {
    public readonly result$ = new Subject<unknown | undefined>();

    constructor(
        private readonly _modalContainer: ComponentRef<ModalContainerComponent>,
        private readonly _modal: ComponentRef<Modal>,
    ) {}

    public closeModal(output?: unknown) {
        this._modalContainer.destroy();
        this._modal.destroy();

        this.result$.next(output);
        this.result$.complete();
    }
}

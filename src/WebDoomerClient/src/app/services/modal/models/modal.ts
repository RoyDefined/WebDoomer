import { ModalContext } from './modal-context';

export abstract class Modal {
    public ɵmodalContext?: ModalContext;

    public close(output?: unknown) {
        this.ɵmodalContext?.closeModal(output);
    }
}

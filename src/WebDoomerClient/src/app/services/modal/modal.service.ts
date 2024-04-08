import { ApplicationRef, ComponentFactory, ComponentFactoryResolver, Injectable, Renderer2, Type } from '@angular/core';
import { ModalContainerComponent } from './components/modal-container-component';
import { Modal } from './models/modal';
import { ModalContext } from './models/modal-context';

@Injectable()
export class ModalService {
    private _modalContainer?: HTMLDivElement;
    private _modalContainerFactory?: ComponentFactory<ModalContainerComponent>;

    constructor(
        private readonly _componentFactoryResolver: ComponentFactoryResolver,
        private readonly _renderer: Renderer2,
        private readonly _appRef: ApplicationRef,
    ) {}

    public openModal<T extends Modal>(component: Type<T>) {
        this.ensureModalContainerExists();

        const modalContainerRef = this._appRef.bootstrap(this._modalContainerFactory!, this._modalContainer);
        const modalComponentRef = modalContainerRef.instance.createModal(component);

        const context = new ModalContext(modalContainerRef, modalComponentRef);
        modalComponentRef.instance.ɵmodalContext = context;

        return context;
    }

    // TODO: This should be validated because there is no guarantee two modals might overwrite eachother.
    private ensureModalContainerExists() {
        const container = this._renderer.createElement('div') as HTMLDivElement;
        this._renderer.appendChild(document.body, container);
        this._modalContainer = container;

        this._modalContainerFactory = this._componentFactoryResolver.resolveComponentFactory(ModalContainerComponent);
    }
}

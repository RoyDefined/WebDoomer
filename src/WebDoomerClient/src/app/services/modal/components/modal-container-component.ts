import { Component, ViewChild, ViewContainerRef, ComponentFactoryResolver, ComponentFactory, Type, ComponentRef } from '@angular/core';
import { Modal } from '../models/modal';

@Component({
    standalone: true,
    selector: 'app-modal-container-component',
    template: `
        <div class="fixed bottom-0 left-0 right-0 top-0 backdrop-blur-sm"></div>
        <div class="fixed left-0 right-0 top-0 z-50 flex h-full max-h-full w-full justify-center overflow-y-auto overflow-x-hidden px-4 pt-[10vh]">
            <div class="relative max-h-full w-full max-w-2xl"><ng-template #modalContainer></ng-template></div>
        </div>
    `,
})
export class ModalContainerComponent {
    @ViewChild('modalContainer', { read: ViewContainerRef }) private modalContainer!: ViewContainerRef;

    constructor(private componentFactoryResolver: ComponentFactoryResolver) {}

    createModal<T extends Modal>(component: Type<T>): ComponentRef<T> {
        this.modalContainer.clear();

        const factory = this.componentFactoryResolver.resolveComponentFactory(component);
        return this.modalContainer.createComponent(factory, 0);
    }
}

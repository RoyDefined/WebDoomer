import { Component, Input, TemplateRef } from '@angular/core';

@Component({
    standalone: true,
    selector: 'app-tab',
    template: '',
})
export class TabComponent {
    /** The header of the tab to display. */
    @Input({ required: true }) headerTemplateRef!: TemplateRef<any>;

    /** The body of the tab to display. */
    @Input({ required: true }) bodyTemplateRef!: TemplateRef<any>;
}

import { Directive, HostListener } from '@angular/core';

@Directive({
    standalone: true,
    selector: '[stop-propagation]',
})
export class StopPropagationDirective {
    @HostListener('click', ['$event'])
    public onClick(event: unknown): void {
        if (event && typeof event === 'object' && 'stopPropagation' in event && typeof event.stopPropagation === 'function') {
            event.stopPropagation();
        }
    }
}

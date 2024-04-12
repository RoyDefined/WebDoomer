import { Directive, EventEmitter, HostListener, Input, Output } from '@angular/core';

/** Represents a directive that allows the user to copy the given content by pressing the element.
 */
@Directive({
    standalone: true,
    selector: '[copy-to-clipboard]',
})
export class CopyToClipboardDirective {
    @Input({ required: true }) content!: string;

    @Output() copied = new EventEmitter<string>();

    @HostListener('click', ['$event'])
    private onClick(event: MouseEvent) {
        event.preventDefault();
        navigator.clipboard.writeText(this.content);
        this.copied.emit(this.content);
    }
}

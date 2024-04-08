import { Directive, ElementRef, HostListener, Input, Renderer2 } from '@angular/core';

@Directive({
    standalone: true,
    selector: '[tooltip]',
})
export class ToolTipDirective {
    @Input({ required: true }) tooltip!: string;
    @Input({ required: false }) placement: 'top' | 'right' | 'bottom' | 'left' = 'top';
    @Input({ required: false }) offset = 10;

    private _tooltipElement?: HTMLSpanElement;

    constructor(
        private readonly _elementRef: ElementRef,
        private readonly _renderer: Renderer2,
    ) {}

    @HostListener('mouseenter')
    private onMouseEnter() {
        if (this._tooltipElement) {
            return;
        }

        const tooltipElement = this.createTooltipElement(this.tooltip);
        const position = this.getPosition(tooltipElement);

        this._renderer.setStyle(tooltipElement, 'top', `${position.top}px`);
        this._renderer.setStyle(tooltipElement, 'left', `${position.left}px`);

        this._tooltipElement = tooltipElement;
    }

    @HostListener('mouseleave')
    private onMouseLeave() {
        if (!this._tooltipElement) {
            return;
        }

        this._renderer.removeChild(document.body, this._tooltipElement);
        this._tooltipElement = undefined;
    }

    private createTooltipElement(text: string) {
        const tooltip = this._renderer.createElement('span') as HTMLSpanElement;
        this._renderer.appendChild(tooltip, this._renderer.createText(text));
        this._renderer.appendChild(document.body, tooltip);

        // General style
        tooltip.style.position = 'absolute';
        tooltip.style.zIndex = '1000';
        tooltip.style.fontSize = '14px';
        tooltip.style.maxWidth = '15rem';
        tooltip.style.textAlign = 'center';
        tooltip.style.background = '#282a36';
        tooltip.style.borderRadius = '4px';
        tooltip.style.pointerEvents = 'none';
        tooltip.style.padding = '.5rem';
        tooltip.style.color = 'white';

        // Placement style
        switch (this.placement) {
            case 'top':
                tooltip.style.top = '100%';
                tooltip.style.left = '50%';
                break;

            case 'right':
                tooltip.style.top = '50%';
                tooltip.style.right = '100%';
                break;

            case 'bottom':
                tooltip.style.bottom = '100%';
                tooltip.style.left = '50%';
                break;

            case 'left':
                tooltip.style.top = '50%';
                tooltip.style.left = '100%';
                break;
        }

        return tooltip;
    }

    getPosition(tooltipElement: HTMLSpanElement) {
        const hostPos = this._elementRef.nativeElement.getBoundingClientRect();
        const tooltipPos = tooltipElement.getBoundingClientRect();

        const scrollPos = window.scrollY || document.documentElement.scrollTop || document.body.scrollTop || 0;

        let top, left;

        if (this.placement === 'top') {
            top = hostPos.top - tooltipPos.height - this.offset;
            left = hostPos.left + (hostPos.width - tooltipPos.width) / 2;
        }

        if (this.placement === 'bottom') {
            top = hostPos.bottom + this.offset;
            left = hostPos.left + (hostPos.width - tooltipPos.width) / 2;
        }

        if (this.placement === 'left') {
            top = hostPos.top + (hostPos.height - tooltipPos.height) / 2;
            left = hostPos.left - tooltipPos.width - this.offset;
        }

        if (this.placement === 'right') {
            top = hostPos.top + (hostPos.height - tooltipPos.height) / 2;
            left = hostPos.right + this.offset;
        }

        return {
            top: top + scrollPos,
            left,
        };
    }
}

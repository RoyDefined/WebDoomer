import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, OnDestroy, TemplateRef, ViewChild } from '@angular/core';
import { HeaderRefService } from '../header-ref.service';

@Component({
    standalone: true,
    selector: 'app-header-bottom-content',
    template: `
        <ng-template #content>
            <ng-content></ng-content>
        </ng-template>
    `,
    imports: [CommonModule],
})
export class HeaderBottomComponent implements AfterViewInit, OnDestroy {
    @ViewChild('content', { static: true })
    public set content(ref: TemplateRef<unknown>) {
        this._ref = ref;
    }

    private _ref?: TemplateRef<unknown>;
    constructor(private readonly _headerRefService: HeaderRefService) {}

    public ngAfterViewInit() {
        Promise.resolve().then(() => {
            console.log('Applying bottom header reference.');
            this._headerRefService.bottomRef = this._ref;
        });
    }

    public ngOnDestroy() {
        this._headerRefService.bottomRef = undefined;
    }
}

import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, OnDestroy, TemplateRef, ViewChild } from '@angular/core';
import { HeaderRefService } from '../header-ref.service';

@Component({
    standalone: true,
    selector: 'app-header-right-content',
    template: `
        <ng-template #content>
            <ng-content></ng-content>
        </ng-template>
    `,
    imports: [CommonModule],
})
export class HeaderRightComponent implements AfterViewInit, OnDestroy {
    @ViewChild('content', { static: true })
    public set content(ref: TemplateRef<unknown>) {
        this._ref = ref;
    }

    private _ref?: TemplateRef<unknown>;
    constructor(private readonly _headerRefService: HeaderRefService) {}

    public ngAfterViewInit() {
        Promise.resolve().then(() => {
            console.log('Applying right header reference.');
            this._headerRefService.rightRef = this._ref;
        });
    }

    public ngOnDestroy() {
        this._headerRefService.rightRef = undefined;
    }
}

import { AfterContentInit, AfterViewInit, Component, ContentChildren, QueryList } from '@angular/core';
import { TabComponent } from './tab.component';
import { CommonModule } from '@angular/common';

@Component({
    standalone: true,
    selector: 'app-tabs',
    template: `
        <div class="border-b border-b-gray-200">
            <ul class="-mb-px flex items-center">
                @for (tabComponent of tabComponents; track $index) {
                    <li class="grow" (click)="onPressTab(tabComponent)">
                        <span
                            class="relative flex cursor-pointer select-none justify-center py-3 text-gray-500 after:absolute after:bottom-0 after:left-0 after:h-0.5 after:w-full hover:text-blue-700 dark:hover:text-blue-500"
                            [ngClass]="{ 'text-blue-700 after:bg-blue-700 dark:text-blue-500 dark:after:bg-blue-500': activeTabComponent == tabComponent }"
                        >
                            <ng-template [ngTemplateOutlet]="tabComponent.headerTemplateRef"></ng-template>
                        </span>
                    </li>
                }
            </ul>
        </div>

        <ng-template [ngTemplateOutlet]="activeTabComponent!.bodyTemplateRef"></ng-template>
    `,
    imports: [CommonModule],
})
export class TabsComponent implements AfterContentInit {
    /** Represents the list of known tabs */
    @ContentChildren(TabComponent) tabComponents!: QueryList<TabComponent>;

    private _activeTabComponent?: TabComponent;
    public get activeTabComponent() {
        return this._activeTabComponent;
    }

    ngAfterContentInit() {
        if (this.tabComponents.length == 0) {
            throw new Error('No tabs were specified. The tabs component requires at least one tab component.');
        }
        this._activeTabComponent = this.tabComponents.first;
    }

    public onPressTab(tabComponent: TabComponent) {
        this._activeTabComponent = tabComponent;
    }
}

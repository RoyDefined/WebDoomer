import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { HeaderComponent } from './header/header.component';
import { FooterComponent } from './footer/footer.component';

@Component({
    standalone: true,
    selector: 'app-layout',
    template: `
        <div class="default-text-color flex h-screen max-h-screen flex-col bg-white dark:bg-gray-800">
            <app-header></app-header>
            <router-outlet></router-outlet>
            <app-footer></app-footer>
        </div>
    `,
    imports: [RouterModule, HeaderComponent, FooterComponent],
})
export class LayoutComponent {}

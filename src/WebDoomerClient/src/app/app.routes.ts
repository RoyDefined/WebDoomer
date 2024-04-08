import { Routes } from '@angular/router';

export const routes: Routes = [
    {
        path: '',
        loadComponent: () => import('./layout/layout.component').then((m) => m.LayoutComponent),
        loadChildren: () => import('./layout/layout.routes').then((m) => m.routes),
    },
];

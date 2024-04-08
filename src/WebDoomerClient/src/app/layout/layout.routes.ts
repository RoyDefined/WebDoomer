import { Routes } from '@angular/router';

export const routes: Routes = [
    {
        path: '',
        loadComponent: () => import('../pages/index/index.component').then((m) => m.IndexComponent),
    },
    {
        path: 'servers',
        loadComponent: () => import('../pages/servers/servers.component').then((m) => m.ServersComponent),
    },
    {
        path: '**',
        redirectTo: '',
        pathMatch: 'full',
    },
];

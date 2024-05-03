import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ServerHubStore } from './stores/signalr/server-hub.store';

@Component({
    standalone: true,
    selector: 'app-root',
    template: `<router-outlet></router-outlet>`,
    imports: [RouterModule],
})
export class AppComponent implements OnInit {
    constructor(private readonly _serverHubStore: ServerHubStore) {}
    ngOnInit() {
        this._serverHubStore.startConnection();
    }
}

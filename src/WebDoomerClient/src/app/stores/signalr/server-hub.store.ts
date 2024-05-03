import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AppSettingsStore } from '../appsettings/app-settings.store';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ServerHubStoreState } from './server-hub-store-state';
import { ComponentStore } from '@ngrx/component-store';
import { Observable, Subject, finalize, switchMap, tap } from 'rxjs';

@Injectable({
    providedIn: 'root',
})
export class ServerHubStore extends ComponentStore<ServerHubStoreState> {
    private _hubConnection?: HubConnection;
    private _onRefreshServers = new Subject<void>();

    public get baseUrl() {
        return this._appSettingsStore.settings?.apiBaseUrl + '/ServerHub';
    }

    private readonly _loading$ = this.select((state) => state.loading);
    private readonly _error$ = this.select((state) => state.error);

    public readonly vm$ = this.select({
        loading: this._loading$,
        error: this._error$,
    });

    public readonly startConnection = this.effect((trigger$) =>
        trigger$.pipe(
            tap(() => this.setLoading(true)),
            switchMap(
                () =>
                    new Observable((subscriber) => {
                        const builder = new HubConnectionBuilder().withUrl(this.baseUrl).withAutomaticReconnect();
                        this._hubConnection = builder.build();
                        this._hubConnection
                            .start()
                            .then(() => {
                                console.log('SignalR connection established with server hub.');
                                subscriber.next();

                                this._hubConnection!.on('OnServerRefresh', () => {
                                    this._onRefreshServers.next();
                                });
                            })
                            .catch((error) => {
                                console.error('Error while starting SignalR connection: ', error);
                                subscriber.next();
                                this.setError(error);
                            });
                    }),
            ),
            finalize(() => this.setLoading(false)),
        ),
    );

    public get onRefreshServers() {
        return this._onRefreshServers;
    }

    private readonly setLoading = this.updater((state, loading: boolean) => {
        return { ...state, loading };
    });
    private readonly setError = this.updater((state, error: Error) => {
        return { ...state, error };
    });

    constructor(private readonly _appSettingsStore: AppSettingsStore) {
        super({
            loading: false,
            error: null,
        });
    }
}

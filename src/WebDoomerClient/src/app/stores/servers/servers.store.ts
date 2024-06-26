import { Injectable } from '@angular/core';
import { ComponentStore, tapResponse } from '@ngrx/component-store';
import { ServersStoreState } from './servers-store-state';
import { ServersApiService } from '../../services/api/servers-api.service';
import { Server } from '../../models/server';
import { EMPTY, Observable, catchError, map, mergeMap, switchMap, tap, withLatestFrom } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import Sqids from 'sqids';
import { ListRange } from '@angular/cdk/collections';

/** Represents the store that handles fetching of servers and its various levels of detail.
 * The store ensures the servers are fetched correctly based on how much data must exist and delegates any errors.
 */
@Injectable({
    providedIn: 'root',
})
export class ServersStore extends ComponentStore<ServersStoreState> {
    private readonly sqidsConverter = new Sqids();

    public readonly servers$ = this.select((state) => state.items);
    public readonly loading$ = this.select((state) => state.loading);
    public readonly searchString$ = this.select((state) => state.searchString);
    public readonly error$ = this.select((state) => state.error);

    public readonly vm$ = this.select({
        servers: this.servers$,
        loading: this.loading$,
        searchString: this.searchString$,
        error: this.error$,
    });

    public readonly getServerIdsWithSearchString = this.effect((searchString$: Observable<string>) =>
        searchString$.pipe(
            map((searchString) => {
                this.setSearchString(searchString);
                this.getServerIds();
            }),
        ),
    );

    public readonly getServerIds = this.effect((trigger$) =>
        trigger$.pipe(
            tap(() => this.setLoading(true)),
            withLatestFrom(this.searchString$),
            switchMap(([_, searchString]) => {
                this.setServers([]);

                return this._serversApiService.getServerIds(searchString).pipe(
                    tapResponse({
                        next: (ids) => {
                            const servers = ids.map((x) => new Server(this.sqidsConverter, x));
                            this.setServers(servers);
                        },
                        error: (error: HttpErrorResponse) => {
                            this.setServers([]);
                            this.setError(error);
                        },
                        finalize: () => this.setLoading(false),
                    }),
                );
            }),
        ),
    );

    public readonly updateListedServersByRange = this.effect((range$: Observable<ListRange>) =>
        range$.pipe(
            withLatestFrom(this.servers$, this.searchString$),
            mergeMap(([range, servers, searchString]) => {
                const take = range.end - range.start;

                return this._serversApiService.GetServersPaginated(range.start, take, searchString).pipe(
                    map((updatedServers) => {
                        for (let i = 0; i < take; i++) {
                            const server = servers[range.start + i];
                            const updatedServer = updatedServers[i];

                            if (!server) {
                                continue;
                            }

                            server.fetching = false;

                            // Ignore received servers that can either not be found anymore or are already patched.
                            if (!updatedServer) {
                                //console.warn(`Failed to find an updated server with index ${i}.`);
                                continue;
                            }

                            // This is possible when loading the server page with a predefined search query.
                            // Rather than updating with new data, ignore any changes to avoid overlapping different states.
                            if (server.state !== 'id') {
                                //console.warn(`Server state not 'id' for server with index ${i}.`);
                                continue;
                            }

                            server.patchAsList(updatedServer);
                        }
                    }),
                    catchError((error) => {
                        this.setError(error);
                        return EMPTY;
                    }),
                );
            }),
            catchError((error) => {
                this.setError(error);
                return EMPTY;
            }),
        ),
    );

    public readonly updateDetailedServer = this.effect((server$: Observable<Server>) =>
        server$.pipe(
            switchMap((server) => {
                if (server.fetching || server.state === 'detailed') {
                    return EMPTY;
                }
                return this._serversApiService.GetServerDetails(server).pipe(
                    map((updatedServer) => {
                        server.patchAsDetailed(updatedServer);
                    }),
                    catchError((error) => {
                        this.setError(error);
                        return EMPTY;
                    }),
                );
            }),
        ),
    );

    private readonly setServers = this.updater((state, items: Server[]) => {
        return { ...state, items };
    });
    private readonly setLoading = this.updater((state, loading: boolean) => {
        return { ...state, loading };
    });
    private readonly setSearchString = this.updater((state, searchString: string) => {
        return { ...state, searchString };
    });
    private readonly setError = this.updater((state, error: Error) => {
        return { ...state, error };
    });

    constructor(private readonly _serversApiService: ServersApiService) {
        super({
            items: [],
            loading: false,
            searchString: null,
            error: null,
        });
    }
}

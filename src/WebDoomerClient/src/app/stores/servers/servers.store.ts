import { Injectable } from '@angular/core';
import { ComponentStore, tapResponse } from '@ngrx/component-store';
import { ServersStoreState } from './servers-store-state';
import { ServersApiService } from '../../services/api/servers-api.service';
import { Server } from '../../models/server';
import { EMPTY, Observable, catchError, combineLatestWith, concatMap, map, mergeMap, switchMap, take, tap } from 'rxjs';
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

    private readonly _servers$ = this.select((state) => state.items);
    private readonly _loading$ = this.select((state) => state.loading);
    private readonly _searchString$ = this.select((state) => state.searchString);
    private readonly _error$ = this.select((state) => state.error);

    public readonly vm$ = this.select({
        servers: this._servers$,
        loading: this._loading$,
        searchString: this._searchString$,
        error: this._error$,
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
            combineLatestWith(this._searchString$),
            switchMap((args) =>
                this._serversApiService.getServerIds(args[1]).pipe(
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
                ),
            ),
        ),
    );

    public readonly updateListedServersByRange = this.effect((range$: Observable<ListRange>) =>
        range$.pipe(
            combineLatestWith(this._servers$),
            concatMap((args) => {
                const [range, servers] = [args[0], args[1]];
                const take = range.end - range.start;

                return this._serversApiService.GetServersPaginated(range.start, take).pipe(
                    map((updatedServers) => {
                        for (let i = 0; i < take; i++) {
                            const server = servers[range.start + i];
                            const updatedServer = updatedServers[i];

                            if (!server) {
                                continue;
                            }

                            server.fetching = false;

                            // This should not trigger.
                            // However, since we fetch data from the server with no reliable reason
                            // to match in size we might as well keep this in.
                            if (!updatedServer) {
                                console.warn(`Failed to find an updated server with index ${i}.`);
                                continue;
                            }

                            if (server.state !== 'id') {
                                console.warn(`Server state not 'id' for server with index ${i}.`);
                                continue;
                            }
                            server.patchAsList(updatedServer);
                        }

                        // This iterates servers that are still marked as 'fetching'.
                        // This should also not happen.
                        for (const server of servers) {
                            if (!server.fetching) {
                                continue;
                            }

                            console.warn(`Server ${server.id} is still marked as fetching.`);
                            server.fetching = false;
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

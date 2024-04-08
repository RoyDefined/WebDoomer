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
    private readonly _error$ = this.select((state) => state.error);

    /** Represents the base number of additional servers that is added to these requests.
     * This does not represent the actual number of servers being fetched.
     * The actual number is calculated after.
     */
    private readonly _serverAdditionalFetchAmount = 50;

    /** Represents the minimum number of servers that must be fetched in order to trigger fetching.
     * A number below this amount will not trigger fetching.
     * This value is ignored if the start/end of a collection is reached, as no more servers come after.
     */
    private readonly _serverMinimumFetchAmount = 30;

    public readonly vm$ = this.select({
        servers: this._servers$,
        loading: this._loading$,
        error: this._error$,
    });

    public readonly getServerIds = this.effect((trigger$) =>
        trigger$.pipe(
            tap(() => this.setLoading(true)),
            switchMap(() =>
                this._serversApiService.getServerIds().pipe(
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
            take(1),
        ),
    );

    public readonly updateListedServersByRangeAndDirection = this.effect((data$: Observable<{ range: ListRange; direction: 'up' | 'down' }>) =>
        data$.pipe(
            combineLatestWith(this._servers$),
            concatMap((args) => {
                const [data, servers] = [args[0], args[1]];
                let range = data.range;
                const direction = data.direction;

                // The directions implement sort of the same system.
                // For readability they have been split.
                // Determine what index to start, and what index to end.
                // One index is based on when the first fetchable index can be found.
                // The other is the last index that might be fetched.
                // After that a take is determined. If this take is too low the fetch won't happen.
                if (direction === 'down') {
                    const startIndex = servers.findIndex((server, index, _) => index >= range.start && !server.fetching && server.state === 'id');
                    if (startIndex == -1) {
                        return EMPTY;
                    }

                    const endIndex = Math.min(range.end + this._serverAdditionalFetchAmount, servers.length);
                    var reachedEnd = endIndex == servers.length;

                    range = { start: startIndex, end: endIndex };
                    var take = range.end - range.start;
                } else {
                    const endIndex = servers.findLastIndex((server, index, _) => index <= range.end && !server.fetching && server.state === 'id');
                    if (endIndex == -1) {
                        return EMPTY;
                    }

                    const startIndex = Math.max(range.start - this._serverAdditionalFetchAmount, 0);
                    var reachedEnd = startIndex == 0;

                    range = { start: startIndex, end: endIndex + 1 };
                    var take = range.end - range.start;
                }

                // Take must be a minimum unless we reached the end of the collection.
                if (!reachedEnd && take < this._serverMinimumFetchAmount) {
                    return EMPTY;
                }

                //console.log('Final fetch range', range);
                //console.log('Take', take);

                // The servers being fetched should indicate they are being fetched.
                for (const server of servers.slice(range.start, range.end)) {
                    server.fetching = true;
                }

                return this._serversApiService.GetServersPaginated(range.start, take).pipe(
                    map((updatedServers) => {
                        for (let i = 0; i < take; i++) {
                            const server = servers[range.start + i];
                            const updatedServer = updatedServers[i];

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
    private readonly setError = this.updater((state, error: Error) => {
        return { ...state, error };
    });

    constructor(private readonly _serversApiService: ServersApiService) {
        super({
            items: [],
            loading: false,
            error: null,
        });
    }
}

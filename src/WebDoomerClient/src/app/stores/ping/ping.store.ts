import { Injectable } from '@angular/core';
import { ComponentStore, tapResponse } from '@ngrx/component-store';
import { ServersApiService } from '../../services/api/servers-api.service';
import { Server } from '../../models/server';
import { EMPTY, Observable, catchError, map, mergeMap, switchMap, tap, withLatestFrom } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import Sqids from 'sqids';
import { ListRange } from '@angular/cdk/collections';
import { PingStoreState } from './ping-store-state';
import { PingApiService } from '../../services/api/ping-api.service';

/** Represents the store that handles communicating with the backend to determine ping and uptime.
 */
@Injectable({
    providedIn: 'root',
})
export class PingStore extends ComponentStore<PingStoreState> {
    private readonly _item$ = this.select((state) => state.item);
    private readonly _loading$ = this.select((state) => state.loading);
    private readonly _error$ = this.select((state) => state.error);

    public readonly vm$ = this.select({
        item: this._item$,
        loading: this._loading$,
        error: this._error$,
    });

    public readonly getPing = this.effect((trigger$) =>
        trigger$.pipe(
            tap(() => this.setLoading(true)),
            switchMap(() => {
                this.setItem(null);
                return this._pingApiService.ping().pipe(
                    tapResponse({
                        next: (item) => {
                            this.setItem(item);
                        },
                        error: (error: HttpErrorResponse) => {
                            this.setError(error);
                        },
                        finalize: () => this.setLoading(false),
                    }),
                );
            }),
        ),
    );

    private readonly setItem = this.updater((state, item: number | null) => {
        return { ...state, item };
    });
    private readonly setLoading = this.updater((state, loading: boolean) => {
        return { ...state, loading };
    });
    private readonly setError = this.updater((state, error: Error) => {
        return { ...state, error };
    });

    constructor(private readonly _pingApiService: PingApiService) {
        super({
            item: null,
            loading: false,
            error: null,
        });
    }
}

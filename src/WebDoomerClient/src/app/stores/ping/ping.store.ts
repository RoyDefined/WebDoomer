import { Injectable } from '@angular/core';
import { ComponentStore, tapResponse } from '@ngrx/component-store';
import { Observable, switchMap, tap } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { PingStoreState } from './ping-store-state';
import { PingApiService } from '../../services/api/ping-api.service';
import { z } from 'zod';
import { appSettingsPingProtocolSchema } from '../appsettings/app-settings-schema';

/** Represents the store that handles communicating with the backend to determine ping and uptime.
 */
@Injectable({
    providedIn: 'root',
})
export class PingStore extends ComponentStore<PingStoreState> {
    public readonly ping$ = this.select((state) => state.ping);
    public readonly loading$ = this.select((state) => state.loading);
    public readonly error$ = this.select((state) => state.error);

    public readonly vm$ = this.select({
        ping: this.ping$,
        loading: this.loading$,
        error: this.error$,
    });

    public readonly getPing = this.effect((protocol$: Observable<z.infer<typeof appSettingsPingProtocolSchema>>) =>
        protocol$.pipe(
            tap(() => this.setLoading(true)),
            switchMap((protocol) => {
                this.setItem(null);
                return this._pingApiService.ping(protocol).pipe(
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

    private readonly setItem = this.updater((state, ping: number | null) => {
        return { ...state, ping };
    });
    private readonly setLoading = this.updater((state, loading: boolean) => {
        return { ...state, loading };
    });
    private readonly setError = this.updater((state, error: Error) => {
        return { ...state, error };
    });

    constructor(private readonly _pingApiService: PingApiService) {
        super({
            ping: null,
            loading: false,
            error: null,
        });
    }
}

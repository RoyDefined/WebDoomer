import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { map, switchMap } from 'rxjs';
import { serverListArraySchema } from './server-list-schema';
import { serverIdListArraySchema } from './server-id-list-schema';
import { Server } from '../../models/server';
import { serverDetailedSchema } from './server-detailed-schema';
import { AppSettingsStore } from '../../stores/appsettings/app-settings.store';
import { z } from 'zod';
import { appSettingsPingProtocolSchema } from '../../stores/appsettings/app-settings-schema';

@Injectable({
    providedIn: 'root',
})
export class PingApiService {
    public get baseUrl() {
        return this._appSettingsStore.settings?.apiBaseUrl + '/';
    }

    constructor(
        private readonly _http: HttpClient,
        private readonly _appSettingsStore: AppSettingsStore,
    ) {}

    public ping(protocol: z.infer<typeof appSettingsPingProtocolSchema>) {
        const now = new Date();
        return this._http.get(this.baseUrl + 'ping/', { observe: 'response' }).pipe(
            map((response) => {
                if (!response.ok) {
                    if (response.body instanceof HttpErrorResponse) {
                        throw new Error(`Response was not positive: ${response.body.message}`);
                    }

                    throw new Error('Response was not positive.');
                }

                if (protocol === 'Timer') {
                    return new Date().getTime() - now.getTime();
                }

                const pingReceiveTime = response.headers.get('Pingreceivetime');
                if (!pingReceiveTime) {
                    return null;
                }

                const parsedDate = new Date(parseInt(pingReceiveTime));
                return new Date().getTime() - parsedDate.getTime();
            }),
        );
    }
}

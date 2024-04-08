import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { map, switchMap } from 'rxjs';
import { serverListArraySchema } from './server-list-schema';
import { serverIdListArraySchema } from './server-id-list-schema';
import { Server } from '../../models/server';
import { serverDetailedSchema } from './server-detailed-schema';
import { AppSettingsStore } from '../../stores/appsettings/app-settings.store';

@Injectable({
    providedIn: 'root',
})
export class ServersApiService {
    public get baseUrl() {
        return this._appSettingsStore.settings?.apiBaseUrl + '/server';
    }

    constructor(
        private readonly _http: HttpClient,
        private readonly _appSettingsStore: AppSettingsStore,
    ) {}

    // TODO: Implement OrderBy
    public getServerIds() {
        return this._http.get(this.baseUrl + '/ids?orderBy=PlayersDescending', { observe: 'response' }).pipe(
            map((response) => {
                if (!response.ok) {
                    if (response.body instanceof HttpErrorResponse) {
                        throw new Error(`Response was not positive: ${response.body.message}`);
                    }

                    throw new Error('Response was not positive.');
                }

                if (response.body === null) {
                    throw new Error('Response body is empty.');
                }

                return serverIdListArraySchema.parse(response.body);
            }),
        );
    }

    // TODO: Implement OrderBy
    public GetServersPaginated(skip: number, take: number) {
        return this._http.get(this.baseUrl + `/range?orderBy=PlayersDescending&skip=${skip}&take=${take}`, { observe: 'response' }).pipe(
            map((response) => {
                if (!response.ok) {
                    if (response.body instanceof HttpErrorResponse) {
                        throw new Error(`Response was not positive: ${response.body.message}`);
                    }

                    throw new Error('Response was not positive.');
                }

                if (response.body === null) {
                    throw new Error('Response body is empty.');
                }

                return serverListArraySchema.parse(response.body);
            }),
        );
    }

    public GetServerDetails(server: Server) {
        return this._http.get(this.baseUrl + `/id/${server.id}`, { observe: 'response' }).pipe(
            map((response) => {
                if (!response.ok) {
                    if (response.body instanceof HttpErrorResponse) {
                        throw new Error(`Response was not positive: ${response.body.message}`);
                    }

                    throw new Error('Response was not positive.');
                }

                if (response.body === null) {
                    throw new Error('Response body is empty.');
                }

                return serverDetailedSchema.parse(response.body);
            }),
        );
    }
}

import { Injectable } from '@angular/core';
import { ComponentStore } from '@ngrx/component-store';
import { HttpClient } from '@angular/common/http';
import { ClientSettingsStore } from '../clientsettings/client-settings.store';
import { EMPTY, catchError, combineLatestWith, finalize, map, of, take, tap } from 'rxjs';
import { formatString } from '../../utils/stringUtils';
import { AppSettingsStoreState } from './app-settings-store-state';
import { z } from 'zod';
import { appSettingsSchema } from './app-settings-schema';

/** Represents the store for the registery file to supply to a user.
 * The store ensures the file is loaded and that is has also been parsed correctly.
 * This file doesn't implement the store pattern as there is no system that updates anything and the file is fetched on app init.
 */
@Injectable({
    providedIn: 'root',
})
export class AppSettingsStore {
    private readonly _appSettingsFileFetch = this._http.get('assets/appsettings.json', { observe: 'response', responseType: 'json' });

    private _settings?: z.infer<typeof appSettingsSchema>;
    public get settings() {
        return this._settings!;
    }

    public readonly fetchAppSettingsFile = this._appSettingsFileFetch.pipe(
        map((response) => {
            if (!response.ok || !response.body) {
                throw new Error('The application settings file could not be fetched correctly.');
            }

            const parseResult = appSettingsSchema.safeParse(response.body);
            if (!parseResult.success) {
                throw new Error('The application settings file could not be parsed correctly.');
            }

            this._settings = parseResult.data;
            console.log('Application settings loaded.', parseResult.data);
        }),
    );

    constructor(private readonly _http: HttpClient) {}
}

import { Injectable } from '@angular/core';
import { ComponentStore } from '@ngrx/component-store';
import { RegisteryFileStoreState } from './registery-file-store-state';
import { HttpClient } from '@angular/common/http';
import { ClientSettingsStore } from '../clientsettings/client-settings.store';
import { EMPTY, catchError, finalize, map, take, tap, withLatestFrom } from 'rxjs';
import { registryFileSchema } from './registery-file-schema';
import { formatString } from '../../utils/stringUtils';

/** Represents the store for the registery file to supply to a user.
 * The store ensures the file is loaded and that is has also been parsed correctly.
 */
@Injectable({
    providedIn: 'root',
})
export class RegisteryFileStore extends ComponentStore<RegisteryFileStoreState> {
    private readonly _file$ = this.select((state) => state.parsedItem || state.baseItem);
    private readonly _baseFile$ = this.select((state) => state.parsedItem || state.baseItem);
    private readonly _loading$ = this.select((state) => state.loading);
    private readonly _error$ = this.select((state) => state.error);

    private readonly _registryFileFetch = this._http.get('assets/Doomseeker-Protocol.txt', { observe: 'response', responseType: 'text' });

    public readonly vm$ = this.select({
        file: this._file$,
        loading: this._loading$,
        error: this._error$,
    });

    public readonly fetchRegistryFile = this.effect((trigger$) =>
        trigger$.pipe(
            tap(() => this.setLoading(true)),
            withLatestFrom(this._registryFileFetch),
            map((args) => {
                const response = args[1];

                if (!response.ok) {
                    this.setError(new Error('The file could not be fetched correctly.'));
                    return;
                }

                const parseResult = registryFileSchema.safeParse(response.body);
                if (!parseResult.success) {
                    this.setError(new Error('The response was not valid.'));
                    return;
                }

                this.setBaseRegisteryFile(parseResult.data);
                this.updateRegisteryFileContent();
            }),
            catchError((error) => {
                this.setError(error);
                return EMPTY;
            }),
            finalize(() => this.setLoading(false)),
            take(1),
        ),
    );

    public readonly updateRegisteryFileContent = this.effect((trigger$) =>
        trigger$.pipe(
            withLatestFrom(this._clientSettingsStore.settings$, this._baseFile$),
            map((args) => {
                const settings = args[1];
                let file = args[2];

                if (!file) {
                    return;
                }

                // Check if we have the doomseeker folder location.
                // Set the current response if not.
                let doomseekerLocation = settings.doomseekerLocation;
                if (!doomseekerLocation) {
                    file = formatString(file, 'Path\\\\To\\\\Doomseeker.exe');
                    this.setParsedRegisteryFile(file);
                    return;
                }

                doomseekerLocation = doomseekerLocation.replaceAll('//', '/').replaceAll('/', '\\').replaceAll('\\\\', '\\').replaceAll('\\', '\\\\');
                file = formatString(file, doomseekerLocation);
                this.setParsedRegisteryFile(file);
            }),
            catchError((error) => {
                this.setError(error);
                return EMPTY;
            }),
        ),
    );

    private readonly setBaseRegisteryFile = this.updater((state, baseItem: string) => {
        return { ...state, baseItem };
    });
    private readonly setParsedRegisteryFile = this.updater((state, parsedItem: string) => {
        return { ...state, parsedItem };
    });
    private readonly setLoading = this.updater((state, loading: boolean) => {
        return { ...state, loading };
    });
    private readonly setError = this.updater((state, error: Error) => {
        return { ...state, error };
    });

    constructor(
        private readonly _http: HttpClient,
        private readonly _clientSettingsStore: ClientSettingsStore,
    ) {
        super({
            baseItem: null,
            parsedItem: null,
            loading: false,
            error: null,
        });
    }
}

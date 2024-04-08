import { Injectable } from '@angular/core';
import { ComponentStore } from '@ngrx/component-store';
import { ClientSettingsStoreState } from './client-settings-store-state';
import { z } from 'zod';
import { take, map, EMPTY } from 'rxjs';
import { clientSettingsSchema } from './client-settings-schema';
import { darkModeTypeSchema } from './darkModeType';

/** Represents the store that handles client settings. */
@Injectable({
    providedIn: 'root',
})
export class ClientSettingsStore extends ComponentStore<ClientSettingsStoreState> {
    public readonly settings$ = this.select((state) => state.items);

    private readonly _localStorageKey = 'WebDoomer.Settings';

    // Used for dark mode.
    // This setting indicates if the client prefers dark mode, as per their system settings.
    private _clientPreferDarkMode = false;

    public readonly fetchClientSettingsFile = this.effect((trigger$) =>
        trigger$.pipe(
            map(() => {
                // Function will return an empty object should fetch/parsing fail.
                // The client will have to redo the settings.

                const settingsJson = localStorage.getItem(this._localStorageKey);
                if (!settingsJson) {
                    this.setSettings({});
                    this.initializeDarkModeListener();

                    console.log('No settings found to load.');
                    return EMPTY;
                }

                // Parse result
                const parseResult = clientSettingsSchema.safeParse(JSON.parse(settingsJson));
                if (!parseResult.success) {
                    this.setSettings({});
                    this.initializeDarkModeListener();

                    console.log('Settings failed to parse.');
                    return EMPTY;
                }

                this.setSettings(parseResult.data);
                this.initializeDarkModeListener();

                console.log('Client settings loaded.', parseResult.data);
                return EMPTY;
            }),
            take(1),
        ),
    );

    public readonly setDarkmodeType = this.updater((state, type: z.infer<typeof darkModeTypeSchema>) => {
        state.items.darkModeType = type;
        this.setSettings(state.items);
        this.updateDarkMode();
        return state;
    });

    public readonly setFormSettings = this.updater(
        (
            state,
            form: Pick<
                z.infer<typeof clientSettingsSchema>,
                'zandronumLocation' | 'qZandronumLocation' | 'doomseekerLocation' | 'iwadsLocation' | 'pwadsLocation'
            >,
        ) => {
            const darkModeType = state.items.darkModeType;
            state.items = {
                ...form,
                darkModeType,
            };
            this.setSettings(state.items);
            return state;
        },
    );

    private readonly setSettings = this.updater((state, items: z.infer<typeof clientSettingsSchema>) => {
        localStorage.setItem(this._localStorageKey, JSON.stringify(items));
        return { ...state, items };
    });

    private initializeDarkModeListener() {
        const preferColorSchemeDarkMedia = window.matchMedia('(prefers-color-scheme: dark)');
        this._clientPreferDarkMode = preferColorSchemeDarkMedia.matches;
        this.updateDarkMode();

        preferColorSchemeDarkMedia.addEventListener('change', (event) => {
            console.log('Dark mode service observed change.');
            this._clientPreferDarkMode = event.matches;
            this.updateDarkMode();
        });
    }

    // Updates dark mode when system options change preferation or something updated the current darkmode value.
    private updateDarkMode() {
        this.settings$.subscribe((settings) => {
            const darkModeClassAdded = document.documentElement.classList.contains('dark');
            const darkModeEnabled = settings.darkModeType === 'Dark' || (settings.darkModeType === 'System' && this._clientPreferDarkMode);

            if (darkModeClassAdded && !darkModeEnabled) {
                document.documentElement.classList.remove('dark');
                return;
            }

            if (!darkModeClassAdded && darkModeEnabled) {
                document.documentElement.classList.add('dark');
                return;
            }
        });
    }

    constructor() {
        super({
            items: {},
        });
    }
}

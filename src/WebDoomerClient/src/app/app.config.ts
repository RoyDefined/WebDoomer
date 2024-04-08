import { APP_INITIALIZER, ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient } from '@angular/common/http';
import { ClientSettingsStore } from './stores/clientsettings/client-settings.store';
import { AppSettingsStore } from './stores/appsettings/app-settings.store';

export const appConfig: ApplicationConfig = {
    providers: [
        provideRouter(routes),
        provideHttpClient(),

        // Configuration setup
        {
            provide: APP_INITIALIZER,
            multi: true,
            deps: [AppSettingsStore],
            useFactory: (appSettingsStore: AppSettingsStore) => () => appSettingsStore.fetchAppSettingsFile,
        },

        // Setup for the settings
        {
            provide: APP_INITIALIZER,
            multi: true,
            deps: [ClientSettingsStore],
            useFactory: (store: ClientSettingsStore) => () => store.fetchClientSettingsFile(),
        },
    ],
};

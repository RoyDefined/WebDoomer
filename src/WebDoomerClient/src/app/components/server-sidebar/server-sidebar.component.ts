import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Server } from '../../models/server';
import { NgOptimizedImage } from '@angular/common';
import { Observable } from 'rxjs';
import { z } from 'zod';
import { clientSettingsSchema } from '../../stores/clientsettings/client-settings-schema';
import { formatString } from '../../utils/stringUtils';
import { isMobile } from '../../utils/isMobile';
import { isWindows } from '../../utils/isWindows';
import { CopyToClipboardDirective } from '../../directives/copy-to-clipboard-directive';

@Component({
    standalone: true,
    selector: 'app-server-sidebar',
    templateUrl: './server-sidebar.component.html',
    imports: [NgOptimizedImage, CopyToClipboardDirective],
})
export class ServerSidebarComponent {
    @Input({ required: true }) server!: Server;
    @Input({ required: true }) settings!: z.infer<typeof clientSettingsSchema>;
    @Output() collapse = new EventEmitter();

    private get baseEngineQuery() {
        const query: string[] = [];
        query.push(`"{0}" -connect "${this.server.ip}:${this.server.port}"`);

        if (this.server.iwad) {
            query.push(`-iwad "{1}${this.server.iwad}"`);
        }

        if (this.server.forcePassword) {
            query.push(`+cl_password "Password"`);
        }

        if (this.server.forceJoinPassword) {
            query.push(`+cl_joinpassword "Password"`);
        }

        if (this.server.pwadCollection && this.server.pwadCollection.length > 0) {
            for (const pwad of this.server.pwadCollection) {
                query.push(`-file "{2}${pwad.name}"`);
            }
        }

        return query.join(' ');
    }

    public get zandronumCommandLineQuery() {
        return formatString(
            this.baseEngineQuery,
            this.settings?.zandronumLocation || 'Path/To/Zandronum.exe',
            this.settings?.iwadsLocation || 'Path/To/IWads/',
            this.settings?.pwadsLocation || 'Path/To/PWads/',
        );
    }

    public get qZandronumCommandLineQuery() {
        return formatString(
            this.baseEngineQuery,
            this.settings?.qZandronumLocation || 'Path/To/QZandronum.exe',
            this.settings?.iwadsLocation || 'Path/To/IWads/',
            this.settings?.pwadsLocation || 'Path/To/PWads/',
        );
    }

    public get commandLineQuery() {
        return this.server.engine === 0 ? this.zandronumCommandLineQuery : this.qZandronumCommandLineQuery;
    }

    public get playerCount() {
        return (this.server.playingClientCount || 0) + (this.server.spectatingClientCount || 0);
    }

    public get botCount() {
        return this.server.botCount || 0;
    }

    public get isMobile() {
        return isMobile();
    }

    public get isWindows() {
        return isWindows();
    }

    public clickCollapse() {
        this.collapse.emit();
    }
}

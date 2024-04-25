import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Server } from '../../models/server';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { z } from 'zod';
import { clientSettingsSchema } from '../../stores/clientsettings/client-settings-schema';
import { formatString } from '../../utils/stringUtils';
import { isMobile } from '../../utils/isMobile';
import { isWindows } from '../../utils/isWindows';
import { CopyToClipboardDirective } from '../../directives/copy-to-clipboard-directive';
import { DomSanitizer } from '@angular/platform-browser';
import { playerSchema } from '../../models/player-schema';
import { ToolTipDirective } from '../../directives/tooltip-directive';
import { pwadSchema } from '../../models/pwad-schema';

@Component({
    standalone: true,
    selector: 'app-server-sidebar',
    templateUrl: './server-sidebar.component.html',
    imports: [NgOptimizedImage, CopyToClipboardDirective, ToolTipDirective, CommonModule],
})
export class ServerSidebarComponent {
    @Input({ required: true }) server!: Server;
    @Input({ required: true }) settings!: z.infer<typeof clientSettingsSchema>;
    @Output() collapse = new EventEmitter();

    private readonly colorCodeMap: { [id: string]: string } = {
        a: 'CC3333',
        b: 'D2B48C',
        c: 'CCCCCC',
        d: '00CC00',
        e: '996633',
        f: 'FFCC00',
        g: 'FF5566',
        h: '9999FF',
        i: 'FFAA00',
        j: 'DFDFDF',
        k: 'EEEE33',
        //l: '',
        m: '000000',
        n: '33EEFF',
        o: 'FFCC99',
        p: 'D1D8A8',
        q: '008C00',
        r: '800000',
        s: '663333',
        t: '9966CC',
        u: '808080',
        v: '00DDDD',
        w: '7C7C98',
        x: 'D57604',
        y: '506CFC',
        z: '236773',
    };

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

    public get passwordUrl() {
        const baseUrl = 'assets/password/{0}-pass-required.png';
        const forcePassword = this.server.forcePassword;
        const forceJoinPassword = this.server.forceJoinPassword;
        return forcePassword && forceJoinPassword
            ? formatString(baseUrl, 'both')
            : forcePassword
              ? formatString(baseUrl, 'connect')
              : forceJoinPassword
                ? formatString(baseUrl, 'join')
                : '';
    }

    public get passwordTip() {
        const forcePassword = this.server.forcePassword;
        const forceJoinPassword = this.server.forceJoinPassword;
        return forcePassword && forceJoinPassword
            ? 'Connect and join password required.'
            : forcePassword
              ? 'Connect password required.'
              : forceJoinPassword
                ? 'Join password required.'
                : '';
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

    constructor(private readonly _sanitizer: DomSanitizer) {}

    public clickCollapse() {
        this.collapse.emit();
    }

    public getPlayerTypeUrl(player: z.infer<typeof playerSchema>) {
        const baseUrl = 'assets/player/player-{0}.png';
        return player.isBot ? formatString(baseUrl, 'bot') : player.isSpectating ? formatString(baseUrl, 'spectator') : formatString(baseUrl, 'regular');
    }

    public getPlayerTypeToolTip(player: z.infer<typeof playerSchema>) {
        return player.isBot ? 'Bot' : player.isSpectating ? 'Spectator' : 'Player';
    }

    public getPwadTypeUrl(pwad: z.infer<typeof pwadSchema>) {
        const baseUrl = 'assets/pwad/pwad-{0}.png';
        return pwad.optional ? formatString(baseUrl, 'optional') : formatString(baseUrl, 'required');
    }

    public getPwadTypeToolTip(pwad: z.infer<typeof pwadSchema>) {
        return pwad.optional ? 'Optional' : 'Required';
    }

    public formatPlayerName(name: string) {
        let output = '';
        let inTag = false;

        // Remove instances of complex colors codes as these are not supported.
        const filteredName = name.replace(/\\c\[[\d\w*]*\]/g, '');

        for (let i = 0; i < filteredName.length; i++) {
            if (filteredName[i] !== '\\' || filteredName[i + 1] != 'c') {
                output += filteredName[i];
                continue;
            }

            if (inTag) {
                output += '</span>';
                inTag = false;
            }

            i += 2;
            const colorCode = filteredName[i];
            if (colorCode == '-' || colorCode == '+' || colorCode == '*' || colorCode == '!') {
                continue;
            }

            const mappedColor = this.colorCodeMap[colorCode];
            if (!mappedColor) {
                continue;
            }

            output += `<span style="color: #${mappedColor};">`;
            inTag = true;
        }

        if (inTag) {
            output += '</span>';
        }

        return this._sanitizer.bypassSecurityTrustHtml(output);
    }
}

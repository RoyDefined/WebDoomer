import { CommonModule, NgOptimizedImage } from '@angular/common';
import { Component, EventEmitter, HostListener, Input, OnChanges, OnInit, Output } from '@angular/core';
import { Server } from '../../models/server';
import { StopPropagationDirective } from '../../directives/stop-propagation-directive';
import { ToolTipDirective } from '../../directives/tooltip-directive';
import { isMobile } from '../../utils/isMobile';
import { isWindows } from '../../utils/isWindows';
import { MediaQuerySize } from '../../utils/media-query-size';

@Component({
    standalone: true,
    selector: 'app-listed-server',
    templateUrl: './listed-server.component.html',
    imports: [CommonModule, StopPropagationDirective, NgOptimizedImage, ToolTipDirective],
})
export class ListedServerComponent implements OnChanges {
    @Input({ required: true }) server!: Server;
    @Input({ required: true }) selected!: Boolean;
    @Input({ required: true }) expandListMediaQuerySize!: MediaQuerySize;
    @Output() clicked = new EventEmitter<Server>();

    public get engineUrl() {
        let engine = this.server.engine;

        // TODO: improve this check.
        const engineFileName = engine === 0 ? 'zandronumsmall' : 'qzandronumsmall';
        const url = `assets/engines/${engineFileName}.png`;
        return url;
    }

    public get flagUrl() {
        // If the country was not supplied, or is `XUN`, display the country as "unknown".
        // The country should not be `XIP` as the server would have geolocated it at this point,
        // but it doesn't hurt to check for it anyway.
        let country = this.server.country?.toLowerCase();
        if (!country || country === 'xip' || country === 'xun') {
            country = 'unknown';
        }
        const countryFlagurl = `assets/flags/${country}.jpg`;
        return countryFlagurl;
    }

    public get clientCount() {
        return (this.server.playingClientCount || 0) + (this.server.spectatingClientCount || 0);
    }

    public get playerCountTitle() {
        const botCount = this.server.botCount;
        const slots = this.server.maxClients;
        let title = 'This server has ';

        if (this.clientCount == 0 && botCount == 0) {
            title += 'no players.';
            return title;
        }

        if (this.clientCount > 0) {
            title += this.clientCount + ' player' + (this.clientCount > 1 ? 's' : '');
        } else {
            title += 'no clients';
        }

        title += ' and ';

        if (botCount && botCount > 0) {
            title += botCount + ' bot' + (botCount > 1 ? 's' : '');
        } else {
            title += 'no bots';
        }

        if (slots && slots > 0) {
            title += ' out of the available ' + slots + ' slot' + (slots > 1 ? 's' : '');
        }

        title += '.';
        return title;
    }

    public get isMobile() {
        return isMobile();
    }

    public get isWindows() {
        return isWindows();
    }

    ngOnChanges() {
        //console.log(`Showing ${this.server.ip}:${this.server.port}`);
    }

    @HostListener('click')
    onClick() {
        // Don't emit fetching servers.
        if (this.server.fetching) {
            return;
        }

        this.clicked.emit(this.server);
    }
}

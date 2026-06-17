import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonComponent } from '../../../../shared/ui/button/button.component';
import { CardComponent } from '../../../../shared/ui/card/card.component';

@Component({
  selector: 'app-unauthorized-page',
  imports: [RouterLink, ButtonComponent, CardComponent],
  templateUrl: './unauthorized-page.component.html',
  styleUrl: './unauthorized-page.component.scss',
})
export class UnauthorizedPageComponent {}

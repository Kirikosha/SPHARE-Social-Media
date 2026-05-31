import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
@Component({
  selector: 'app-edit-main-info',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './edit-main-info.component.html',
  styleUrl: './edit-main-info.component.css'
})
export class EditMainInfoComponent {
}

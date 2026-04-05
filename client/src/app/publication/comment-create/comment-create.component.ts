import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { CreateCommentModel } from '../../_models/commentModel';

@Component({
  selector: 'app-comment-create',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './comment-create.component.html',
  styleUrl: './comment-create.component.css'
})
export class CommentCreateComponent {
  @Input() publicationId!: string;
  @Output() commentCreated = new EventEmitter<CreateCommentModel>();

  commentForm: FormGroup;

  constructor(private fb: FormBuilder) {
    this.commentForm = this.fb.group({
      content: ['', [Validators.required, Validators.maxLength(500)]]
    });
  }

  onSubmit(): void {
    if (this.commentForm.invalid) return;

    const comment: CreateCommentModel = {
      content: this.commentForm.value.content,
      publicationId: this.publicationId
    };

    this.commentCreated.emit(comment);
    this.commentForm.reset();
  }

  get content() { return this.commentForm.get('content'); }
}

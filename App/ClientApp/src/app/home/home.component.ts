import { Component, OnInit,  EventEmitter, Output} from '@angular/core';
import { DecodeHtmlEntitiesModule} from 'decode-html-entities';
import { ApiService } from '../api.service';
import {StripHtmlPipe} from '../../../pipes/striphtml.pipe';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
  providers: [ StripHtmlPipe ]
})

export class HomeComponent implements OnInit {

  public results;
  public searchValue;

  constructor(private apiService: ApiService) { }

  ngOnInit() {

  }

  onSubmit(searchValue: string) {
    this.searchValue = searchValue;
    this.apiService.getDocuments(searchValue).subscribe((data)=>{
      this.results = data;
    });
  }


}

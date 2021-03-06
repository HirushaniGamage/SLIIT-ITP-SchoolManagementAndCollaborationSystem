import { DropdownService } from 'src/app/services/drop-down/dropdown.service';
import { DropDownModel } from 'src/app/models/common/drop-down.model';
import Swal from 'sweetalert2';
import { Upload } from './../../models/common/upload';
import { EMPTY, Observable } from 'rxjs';
import { StudentService } from './../../services/student/student.service';
import { NgxSpinnerService } from 'ngx-spinner';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-excel-upload',
  templateUrl: './excel-upload.component.html',
  styleUrls: ['./excel-upload.component.sass']
})
export class ExcelUploadComponent implements OnInit {

  excelTypes:DropDownModel[]=[];

  constructor(
    private studentService:StudentService,
    private spinner:NgxSpinnerService,
    private dropDownServie:DropdownService,
    ) { }



  /*fileTypes:any[]= [
    {id:1,name:"Student Excel"},
    {id:2,name:"Management Level User Excel"}
  ];*/
  selectedExcelFileTypeId=1;

  ngOnInit(): void {
    this.getExcelMasrerData();
  }

  excelTypeOnChanged(item:any)
  {

  }

  upload$: Observable<Upload> = EMPTY;
  precentage:any;
  progressBarVisible:boolean=false;
  onFileChange(event: any,) 
  {

    this.progressBarVisible=true;
    let fi = event.srcElement;
    const formData = new FormData();
    //formData.set("id",this.quotationId.toString());

    if(fi.files.length>0)
    {
        for (let index = 0; index < fi.files.length; index++) {
          
          formData.append('file', fi.files[index], fi.files[index].name);
        }

        this.spinner.show();
        this.studentService.uploadClassStudents(formData).subscribe(res=>
          {
            this.precentage =res;
            if(res.state=="DONE")
            {
              this.spinner.hide();
              this.progressBarVisible=false;

              Swal.fire({
                icon: 'success',
                title: 'Done',
                text: 'Excel file has been uploaded',
              });
            }
            //progress
          },error=>{
              this.spinner.hide();
          });

    }    
  }

  getExcelMasrerData(){
      this.dropDownServie.getExcelMasrerData().subscribe(response=>{
         this.excelTypes = response;
      })
  }

}

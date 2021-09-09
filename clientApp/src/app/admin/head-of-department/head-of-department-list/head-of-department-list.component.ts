import { DropDownModel } from './../../../models/common/drop-down.model';
import { DatatableComponent } from '@swimlane/ngx-datatable';
import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ToastrService } from 'ngx-toastr';
import Swal from 'sweetalert2';
import { HeadOfDepartmentService } from './../../../services/head-of-department/head-of-department.service';
import { HeadOfDepartmentModel } from 'src/app/models/head-of-department/head.of.department.model';

@Component({
  selector: 'app-head-of-department-list',
  templateUrl: './head-of-department-list.component.html',
  styleUrls: ['./head-of-department-list.component.sass'],
  providers: [ToastrService],
})
export class HeadOfDepartmentListComponent implements OnInit {

  @ViewChild(DatatableComponent, { static: false }) table: DatatableComponent;
  data = [];
  scrollBarHorizontal = window.innerWidth < 1200;
  loadingIndicator = false;
  headOfDepartmentFrom:FormGroup;
  subjects:DropDownModel[] = [];
  academicYears:DropDownModel[] = [];
  academicLevels:DropDownModel[] = [];
  teachers:DropDownModel[]=[];
  reorderable = true;

  constructor(
    private fb: FormBuilder,
    private modalService: NgbModal,
    private headOfDepartmentService:HeadOfDepartmentService,
    private toastr: ToastrService) { }

  ngOnInit(): void {
    this.getAll();
    this.getAllAcademicYears();
    this.getAllAcademicLevels();
    this.getAllTeachers();
    this.getAllSubjects();
  }

  getAllAcademicYears()
  {
    this.headOfDepartmentService.getAllAcademicYears()
      .subscribe(response=>
      { this.academicYears = response;
      },error=>{
        this.toastr.error("Network error has been occured. Please try again.","Error");
       });
  }

  getAllAcademicLevels()
  {
    this.headOfDepartmentService.getAllAcademicLevels()
      .subscribe(response=>
      { this.academicLevels = response;
      },error=>{
        this.toastr.error("Network error has been occured. Please try again.","Error");
       });
  }

  getAllTeachers()
  {
    this.headOfDepartmentService.getAllTeachers()
      .subscribe(response=>
      { this.teachers = response;
      },error=>{
        this.toastr.error("Network error has been occured. Please try again.","Error");
       });
  }

  getAllSubjects()
  {
    this.headOfDepartmentService.getAllSubjects()
      .subscribe(response=>
      { this.subjects = response;
      },error=>{
        this.toastr.error("Network error has been occured. Please try again.","Error");
       });
  }

  getAll()
  {
    this.loadingIndicator=true;
    this.headOfDepartmentService.getAll()
    .subscribe(response=>
    {
        this.data= response;
        this.loadingIndicator=false;
    },error=>{
      this.loadingIndicator=false;
      this.toastr.error("Network error has been occured. Please try again.","Error");
    });
  }


  addNewHeadOfDepartment(content) {

    this.headOfDepartmentFrom = this.fb.group({
      id:[0],
      academicYearId: ['', [Validators.required]],
      academicLevelId: [null, [Validators.required]],
      teacherId: [null, [Validators.required]]
     // subjectId: [null, [Validators.required]]
    });

    this.modalService.open(content, {
      ariaLabelledBy: 'modal-basic-title',
      size: 'lg',
    });

  }

  saveHeadOfDepartment(){   
    
    console.log(this.headOfDepartmentFrom.value);
    
    this.headOfDepartmentService.saveHeadOfDepartment(this.headOfDepartmentFrom.value)
    .subscribe(response=>{

        if(response.isSuccess)
        {
          this.modalService.dismissAll();
          this.toastr.success(response.message,"Success");
          this.getAll();
        }
        else
        {
          this.toastr.error(response.message,"Error");
        }

    },error=>{
      this.toastr.error("Network error has been occured. Please try again.","Error");
    });

  }


    onAddRowSave(form: FormGroup) {
      this.data.push(form.value);
      this.data = [...this.data];
      form.reset();
      this.modalService.dismissAll();
      this.addRecordSuccess();
    }


  editRow(row:HeadOfDepartmentModel, rowIndex:number, content:any) {

    console.log(row);
    
    this.headOfDepartmentFrom = this.fb.group({
      id:[row.id],
      academicYearId: [row.academicYearId, [Validators.required]],
      academicLevelId: [row.academicLevelId, [Validators.required]],
      teacherId: [row.teacherId, [Validators.required]],
      subjectId: [row.subjectId, [Validators.required]],
    });

    this.modalService.open(content, {
      ariaLabelledBy: 'modal-basic-title',
      size: 'lg',
    });
  }

  //delete Head Of Department
  deleteHeadOfDepartment(row) {
    Swal.fire({
      title: 'Are you sure Delete Head Of Department ?',
      showCancelButton: true,
      confirmButtonColor: 'red',
      cancelButtonColor: 'green',
      confirmButtonText: 'Yes',
    }).then((result) => {

      if (result.value) {

        this.headOfDepartmentService.delete(row.id).subscribe(response=>{

          if(response.isSuccess)
          {
            this.toastr.success(response.message,"Success");
            this.getAll();
          }
          else
          {
            this.toastr.error(response.message,"Error");
          }
    
        },error=>{
          this.toastr.error("Network error has been occured. Please try again.","Error");
        });
      }
    });
  }
  
    addRecordSuccess() {
      this.toastr.success('Acedemic Level Add Successfully', '');
    }

}

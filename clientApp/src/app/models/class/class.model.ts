import { Injectable } from '@angular/core';

@Injectable()
export class ClassModel{
    classNameId:number;
    classNameName:string;
    academicLevelId:number;
    academicLevelName:string;
    academicYearId:number;
    name:string;
    classCategory:number;
    classCategoryName:string;
    languageStream:number;
    languageStreamName:string;
    createdOn:Date;
    createdById:number;
    createdByName:string;
    updatedOn:Date;
    updatedById:number;
    updatedByName:string;
    selectedClassNameId:number;
    selectedAcademicLevelId:number;
    selectedClassCategory:number;
    selectedLanguageStream:number;
}
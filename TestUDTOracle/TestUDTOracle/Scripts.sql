﻿--drop table PRODUCT
CREATE TABLE PRODUCT(
ID NUMBER(20,0),
DESCRIPTION VARCHAR2(100),
PRICE NUMBER(20,2));

/*Creación del UDT*/
--drop type PRODUCT_UDT force;
create or replace TYPE PRODUCT_UDT AS OBJECT(
ID NUMBER(20),
DESCRIPTION VARCHAR2(100),
PRICE NUMBER(20,2)) NOT FINAL INSTANTIABLE;

/*Creación de la tabla UDT*/
create or replace TYPE PRODUCT_TABLE_UDT AS TABLE OF PRODUCT_UDT;

CREATE OR REPLACE PACKAGE PKG_TEST_01 AS 
  PROCEDURE CREATE_PRODUCT(
    P_PRODUCTS PRODUCT_TABLE_UDT
  );
END PKG_TEST_01;

CREATE OR REPLACE PACKAGE BODY PKG_TEST_01 AS 
  PROCEDURE CREATE_PRODUCT(
    P_PRODUCTS PRODUCT_TABLE_UDT
  )AS
  BEGIN
    IF(P_PRODUCTS.COUNT>0)THEN
      FOR i IN P_PRODUCTS.FIRST .. P_PRODUCTS.LAST
      LOOP
        INSERT INTO PRODUCT(ID,DESCRIPTION,PRICE) 
        VALUES(P_PRODUCTS(i).ID,P_PRODUCTS(i).DESCRIPTION,P_PRODUCTS(i).PRICE);
      END LOOP;
    END IF;
  END;
END PKG_TEST_01;

DELETE FROM PRODUCT;
COMMIT;

SELECT ID,DESCRIPTION,TO_CHAR(PRICE) PRICE FROM PRODUCT;
# Generate Request and Certificate PKI Application

Primary this application was develop for creating certificate request for web server with additional subject names.
Generated certificate file with .csr extension need to be send to internal or external certificate authorities (CA) to generate certificate file. This generated certificate file will be connecting with certificate private key to generate .pfx certificate file for web server.
Certificate file generated on this way with internal or external CA public key certificate will be used on web server to provide web server authentication.

After initial version, I added new option for generating self-sign certificate that can be generate root CA certificate and other option for generating certificate base on date inside certificate request file (.csr).

This application can be used for practical and educational purpose.
	
This is Windows Desktop application developed in C#, using WPF MahApps and Bouncy Castle PKI libraries.

## How to use

You can start application and choose between 4 menu options. For complete help read user manual.

## User manual
	
Complete help with screenshots you can find inside "GenCertHelp" folder in different file formats.
I used chmProcessor tool from location http://chmprocessor.sourceforge.net/ to generate different file formats from MS Word document.

To generate this type of help documentations you need to download software and make chmProcessor project file (file with .WHC extension inside "GenCertHelp" folder).

## How to successfully build application from source code

If you deleted or changed help documentation inside GenCertHelp folder, must build help documentation inside that folder.
To successfully build documentation use chmProcessor tool from location http://chmprocessor.sourceforge.net/.
Then rebuild Visual Studio project.
All complete binary distribution you will found inside out folder of your Visual Studio project directory.

-----------------------------------------------------------------------------------
CZ:

Nově v nabídce "Naprosto unikátní EASY SYSTEM BUILDER" 
absolutně snadná tvorba 3vrstvého systému se znalostmi excelu
Stačí vytvořit Tabulku pro Data a Formulář, vše v grafických designerech
Systém si tedy doslova naklikáte, Každý den 1 tabulka = za měsíc prodejní systém i s objednávkami a nabídkami
A TO ANI NÁHODOU NENÍ VŠE.
Nemusí to být pouze systém Datový, ale bez probléMu i multimediální, na stříhání videí, práce s fotkami či 3D
nebo Dokonce SYSTÉM ŘÍDÍCÍ pro ovládání strojů (Aktuálně je podpora PLC SIEMENS), nebo libovolného jiného Hardware
po dodání specifik. 
Či dále systém Kontrolní, Zálohovací, DataWarehouse, Controling, BI OLAP, Flow Procesy, 
Výrobní, Informační či business dotykové Terminály.

Zahajovací náklad za jádro je pouhých 10 000Kč / 400Euro , a můžete si zkoušet sami či levně doobjednat
co tam budete chtít přidat. Neohýbejte se před systémem vy, POŘIĎTE SI SYSTÉM PŘESNĚ NA MÍRU.
Pro představu:
 - fakturační systém s OBJ+NAB+MULTIPOBOČKY,SKLAD,POKLADNA                        - 14 DNÍ
 - implementace dotykového terminálu do výroby pro výkazy práce                   - 4 DNY
 - Právěš vzniká systém pro hotely včetně pokojové agengy,fakturace a objednávek  - 1 MĚSÍC
 
 Neomezený počet uživatelů, neomezený tisk, neomezené možnosti, návody, tipy, triky, rady,
 Připraveno pro všechny typy systémů a terminálů pro MS WINDOWS
 
INTELIGENTNÍ DOKUMENTACE: https://liborsvoboda.github.io/EASYSYSTEM-EASYSERVER-CZ/
KÓD: https://github.com/liborsvoboda/EASYSYSTEM-EASYSERVER-CZ

 TAK NEOTÁLEJTE A OZVĚTE SE JEŠTĚ DNES
 
 IT Architekt
 Libor Svoboda GroupWare-Solution.Eu
 Tel: 00420 724986873, email: Libor.Svoboda@GroupWare-Solution.Eu
 
 
 
----------------------------------------------------------------------------------------
EN: 
 
New in the menu "Absolutely unique EASY SYSTEM BUILDER"
absolutely easy to create a 3-tier system with excel knowledge
Just create a Table for Data and a Form, all in graphic designers
So you literally click on the system, 1 table every day = sales system with orders and offers per month
And that's not all, by any chance.
It doesn't have to be only a data system, but also a multimedia one without any problem, for cutting videos, working with photos or 3D
or even a CONTROL SYSTEM for machine control (Currently SIEMENS PLC is supported), or any other Hardware
after delivery of specifications.
Or the Control system, Backup, DataWarehouse, Controlling, BI OLAP, Flow Processes,
Production, Information or business touch Terminals.

The starting cost for the core is only 10,000 CZK / 400 Euro, and you can try it yourself or order cheaply
what you want to add there. Don't bow down to the system, GET A CUSTOMIZED SYSTEM.
For idea:
  - invoicing system with OBJ+NAB+MULTI BRANCHES, WAREHOUSE, CHECKOUT - 14 DAYS
  - implementation of a touch terminal in production for work reports - 4 DAYS
  - A system for hotels including room agency, invoicing and orders is currently being created - 1 MONTH
 
  Unlimited users, unlimited printing, unlimited options, tutorials, tips, tricks, advice,
  Prepared for all types of systems and terminals for MS WINDOWS
 
INTELLIGENT DOCUMENTATION: https://liborsvoboda.github.io/EASYSYSTEM-EASYSERVER-EN/
CODE: https://github.com/liborsvoboda/EASYSYSTEM-EASYSERVER-EN

  SO DON'T DELAY AND CALL TODAY
 
  IT Architect
  Libor Svoboda GroupWare-Solution.Eu
  Tel: 00420 724986873, email: Libor.Svoboda@GroupWare-Solution.Eu

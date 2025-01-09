# APRT (Assisted PIN Remove Tool)

This is a simple tool that allows you to disable CHV/PIN from SIM cards in bulk via PC/SC interface for smart cards.

### How to use:
 
- Insert the smart card reader and the SIM card.
- Start the program and press ENTER.
- Enter the current PIN of the SIM card.
- Press ENTER to unlock the SIM card.


### Bulk:

You can place one or more `ICCID,PIN,PUK` relationship **CSV** files to the `rel/` directory to automatically unlock the SIM card's PIN without requiring user input.
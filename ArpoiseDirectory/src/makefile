# Makefile for the cgi programm, you must copy the pbl src directory *.c and *.h files into this directory before you can make
#
AR=      /usr/bin/ar
RANLIB=  /usr/bin/ar ts
IPATH=   -I.
CFLAGS=  -Wall -O3 -std=c99 ${IPATH}
CC= gcc

INCLIB    = 

LIB_OBJS  = pblCgi.o pblStringBuilder.o pblPriorityQueue.o pblHeap.o pblMap.o pblSet.o pblList.o pblCollection.o pblIterator.o pblhash.o pbl.o
THELIB    = libpbl.a

EXE_OBJS1 = ArpoiseDirectory.o
THEEXE1   = ArpoiseDirectory.cgi

all: $(THELIB) $(THEEXE1)

$(THELIB):  $(LIB_OBJS)
	$(AR) rc $(THELIB) $?
	$(RANLIB) $(THELIB)

$(THEEXE1):  $(EXE_OBJS1) $(THELIB)
	$(CC) -O2 -o $(THEEXE1) $(EXE_OBJS1) $(THELIB) $(INCLIB)

clean:
	rm -f ${THELIB}  ${LIB_OBJS} core
	rm -f ${THEEXE1} ${EXE_OBJS1}


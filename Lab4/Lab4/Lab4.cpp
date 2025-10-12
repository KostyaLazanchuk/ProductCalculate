#include "mpi.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

int main(int argc, char* argv[]) {
    int size = 0, rank = -1;

    MPI_Init(&argc, &argv);

    MPI_Comm_size(MPI_COMM_WORLD, &size);
    MPI_Comm_rank(MPI_COMM_WORLD, &rank);

    if (rank == 0) {
        printf("Amount of tasks = %d\n", size);
        fflush(stdout);
    }
    printf("[rank=%d] My number in MPI_COMM_WORLD = %d\n", rank, rank);
    fflush(stdout);

    MPI_Barrier(MPI_COMM_WORLD);

    if (rank == 0) {
        puts("CommandLine for task 0:");
        for (int i = 0; i < argc; i++) {
            printf("%d: \"%s\"\n", i, argv[i]);
        }
        fflush(stdout);
    }

    const char* parity = (rank % 2 == 0) ? "even" : "odd";
    printf("[rank=%d] Variant1: my rank is %s\n", rank, parity);
    fflush(stdout);


    int has_variant2 = 0;
    int workstation = -1;
    const char* surname = NULL;

    if (argc >= 3) {
        char* endptr = NULL;
        long ws = strtol(argv[1], &endptr, 10);
        if (endptr != argv[1] && *endptr == '\0' && ws >= 0) {
            workstation = (int)ws;
            surname = argv[2];
            has_variant2 = 1;
        }
    }

    MPI_Barrier(MPI_COMM_WORLD);

    if (has_variant2) {
        if (rank == workstation) {
            printf("[rank=%d] Variant2: %s\n", rank, surname);
        }
        else {
            printf("[rank=%d] Variant2: %d\n", rank, rank);
        }
        fflush(stdout);
    }
    else {
        if (rank == 0) {
            puts("[info] Variant2 skipped: pass <workstation> <surname> as arguments to enable it.");
            fflush(stdout);
        }
    }

    MPI_Finalize();
    return 0;
}

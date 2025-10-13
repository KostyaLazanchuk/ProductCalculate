#define _CRT_SECURE_NO_WARNINGS
#include <mpi.h>
#include <stdio.h>
#include <string.h>
#include <math.h>


#ifndef M_PI
#define M_PI 3.14159265358979323846
#endif

int main(int argc, char* argv[]) {
    int rank, size;
    MPI_Init(&argc, &argv);
    MPI_Comm_rank(MPI_COMM_WORLD, &rank);
    MPI_Comm_size(MPI_COMM_WORLD, &size);

    char surname[50] = "Lazanchuk";
    char fullname[100];
    int length = 0;
    double sumCodes = 0.0, result = 0.0;

    if (rank == 0) {
        for (int i = 1; i < size; i++) {
            MPI_Send(surname, strlen(surname) + 1, MPI_CHAR, i, 0, MPI_COMM_WORLD);
        }

        char fullFrom1[100];
        MPI_Recv(fullFrom1, 100, MPI_CHAR, 1, 0, MPI_COMM_WORLD, MPI_STATUS_IGNORE);
        MPI_Recv(&length, 1, MPI_INT, 2, 0, MPI_COMM_WORLD, MPI_STATUS_IGNORE);
        MPI_Recv(&result, 1, MPI_DOUBLE, 3, 0, MPI_COMM_WORLD, MPI_STATUS_IGNORE);

        printf("Process 0 get:\n");
        printf("1 Full name: %s\n", fullFrom1);
        printf("2 Length: %d symbols\n", length);
        printf("3 Sum * pi: %.2f\n", result);
    }

    else if (rank == 1) {
        char recvBuf[50];
        MPI_Recv(recvBuf, 50, MPI_CHAR, 0, 0, MPI_COMM_WORLD, MPI_STATUS_IGNORE);
        sprintf(fullname, "%s %s", recvBuf, "Kostiantyn");
        MPI_Send(fullname, strlen(fullname) + 1, MPI_CHAR, 0, 0, MPI_COMM_WORLD);
    }

    else if (rank == 2) {
        char recvBuf[50];
        MPI_Recv(recvBuf, 50, MPI_CHAR, 0, 0, MPI_COMM_WORLD, MPI_STATUS_IGNORE);
        length = strlen(recvBuf);
        MPI_Send(&length, 1, MPI_INT, 0, 0, MPI_COMM_WORLD);
    }

    else if (rank == 3) {
        char recvBuf[50];
        MPI_Recv(recvBuf, 50, MPI_CHAR, 0, 0, MPI_COMM_WORLD, MPI_STATUS_IGNORE);
        for (int i = 0; i < strlen(recvBuf); i++) {
            sumCodes += (int)recvBuf[i];
        }
        result = sumCodes * M_PI;
        MPI_Send(&result, 1, MPI_DOUBLE, 0, 0, MPI_COMM_WORLD);
    }

    MPI_Finalize();
    return 0;
}

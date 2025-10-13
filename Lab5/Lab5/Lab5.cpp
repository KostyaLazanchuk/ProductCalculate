#include <mpi.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdbool.h>

static bool parse_args(int argc, char** argv, int* variant, int* ws) {
    *variant = 0;
    *ws = 0;
    for (int i = 1; i < argc; ++i) {
        if (strncmp(argv[i], "--variant=", 10) == 0) {
            *variant = atoi(argv[i] + 10);
        }
        else if (strncmp(argv[i], "--ws=", 5) == 0) {
            *ws = atoi(argv[i] + 5);
        }
    }
    return (*variant == 1 || *variant == 2) && (*ws > 0);
}

int main(int argc, char* argv[]) {
    int size = 0, rank = -1;
    int variant = 0, ws = 0;

    MPI_Init(&argc, &argv);
    MPI_Comm_size(MPI_COMM_WORLD, &size);
    MPI_Comm_rank(MPI_COMM_WORLD, &rank);

    if (size != 2) {
        if (rank == 0) {
            fprintf(stderr, "Only 2 tasks required instead of %d, abort\n", size);
        }
        MPI_Barrier(MPI_COMM_WORLD);
        MPI_Abort(MPI_COMM_WORLD, MPI_ERR_OTHER);
    }

    if (!parse_args(argc, argv, &variant, &ws)) {
        if (rank == 0) {
            fprintf(stderr, "Usage: mpirun -n 2 ./prog --variant=1|2 --ws=<positive_int>\n");
            fprintf(stderr, "Variant 1: type=long,  buf_len = ws*10, send_count = ws+1\n");
            fprintf(stderr, "Variant 2: type=float, buf_len = ws*11, send_count = ws+1\n");
        }
        MPI_Barrier(MPI_COMM_WORLD);
        MPI_Abort(MPI_COMM_WORLD, MPI_ERR_OTHER);
    }

    const int TAG = 100;

    if (variant == 1) {
        const int buf_len = ws * 10;
        const int send_count = ws + 1;

        if (rank == 1) {
            long* buf = (long*)malloc(sizeof(long) * buf_len);
            if (!buf) {
                fprintf(stderr, "[rank %d] malloc failed\n", rank);
                MPI_Abort(MPI_COMM_WORLD, MPI_ERR_OTHER);
            }
            for (int i = 0; i < send_count; ++i) buf[i] = (long)(i * 10L);

            MPI_Send(buf, send_count, MPI_LONG, 0, TAG, MPI_COMM_WORLD);
            free(buf);
        }
        else if (rank == 0) {
            long* buf = (long*)malloc(sizeof(long) * buf_len);
            if (!buf) {
                fprintf(stderr, "[rank %d] malloc failed\n", rank);
                MPI_Abort(MPI_COMM_WORLD, MPI_ERR_OTHER);
            }
            MPI_Status status;
            MPI_Recv(buf, buf_len, MPI_LONG, 1, TAG, MPI_COMM_WORLD, &status);

            int received = 0;
            MPI_Get_count(&status, MPI_LONG, &received);
            printf("[rank 0] Variant 1: ws=%d, buf_len=%d, expected<=%d, received=%d elements (type long)\n",
                ws, buf_len, buf_len, received);

            int show = received < 5 ? received : 5;
            printf("[rank 0] First %d values:", show);
            for (int i = 0; i < show; ++i) printf(" %ld", buf[i]);
            printf("\n");

            free(buf);
        }
    }
    else {
        const int buf_len = ws * 11;
        const int send_count = ws + 1;

        if (rank == 1) {
            float* buf = (float*)malloc(sizeof(float) * buf_len);
            if (!buf) {
                fprintf(stderr, "[rank %d] malloc failed\n", rank);
                MPI_Abort(MPI_COMM_WORLD, MPI_ERR_OTHER);
            }
            for (int i = 0; i < send_count; ++i) buf[i] = (float)(i + 0.5f);

            MPI_Send(buf, send_count, MPI_FLOAT, 0, TAG, MPI_COMM_WORLD);
            free(buf);
        }
        else if (rank == 0) {
            float* buf = (float*)malloc(sizeof(float) * buf_len);
            if (!buf) {
                fprintf(stderr, "[rank %d] malloc failed\n", rank);
                MPI_Abort(MPI_COMM_WORLD, MPI_ERR_OTHER);
            }
            MPI_Status status;
            MPI_Recv(buf, buf_len, MPI_FLOAT, 1, TAG, MPI_COMM_WORLD, &status);

            int received = 0;
            MPI_Get_count(&status, MPI_FLOAT, &received);
            printf("[rank 0] Variant 2: ws=%d, buf_len=%d, expected<=%d, received=%d elements (type float)\n",
                ws, buf_len, buf_len, received);

            int show = received < 5 ? received : 5;
            printf("[rank 0] First %d values:", show);
            for (int i = 0; i < show; ++i)
            {
                printf(" %.2f", buf[i]);
            }

            printf("\n");

            free(buf);
        }
    }

    MPI_Finalize();
    return 0;
}

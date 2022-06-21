<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> 119575f207e051d8ecdd2881e582360997e4794c
﻿function moreDetail(id) {
    $.get('/Venta/getMoreDetail/' + id, function (data) {
        if (data != null) {
            abrirModal(data);
        }
        console.log(data);
    });
<<<<<<< HEAD
}
=======
}
=======
﻿
>>>>>>> 963d1d0eb548451d125fa1f3118e08871080c9fd
>>>>>>> 119575f207e051d8ecdd2881e582360997e4794c

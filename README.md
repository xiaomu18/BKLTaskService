# BKLTaskService
启动过程:
1. 系统启动 BKLTaskService
2. 挑选随机服务名
3. 检测是否安装了 BKLTaskController
4. 创建启动脚本
5. 启动 BKLTaskController --> 设置环境变量，伪装参数至 python update
6. 退出 BKLTaskService

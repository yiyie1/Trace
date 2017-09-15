using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trace
{
    class Constants
    {
        public const String AJUSTING_ANGLE = "光谱仪正在调整角度至";

        public const String MODIFY_STOP1 = "请修改光阑直径至:";
        public const String MODIFY_STOP2 = "，然后点击“确定”按钮";

        public const String MOVING_TO_AJUST_POS = "光谱仪正在移动到校准位置，请勿碰触";

        public const String SAVE_COMPENSATE_PARAMS_TO_DATABASE_SUCCESS = "补偿参数导入成功";

        public const String SAVE_COMPENSATE_PARAMS_CSV_SUCCESS = "补偿参数导出成功";

        public const String WHETHER_SAVE_PARAMS = "当前测量参数会被覆盖，确认是否继续？";

        public const String NO_TEST_PARAMS = "当前测量参数为空，请重新输入";

        public const String IMPORT_PARAMS_WRONG = "导入的测量计划文件格式错误，请检查CSV文件格式";

        public const String AJUST_PARAM_NO_EXIST = "存在未获取校准参数的测量参数，请重新设置测量参数后校准";

        public const String WARNING = "警告";

        public const String WRONG = "错误";

        public const String TIP = "提示";

        public const String MILISECOND = "毫秒";

        public const String SECOND = "秒";

        public const String CAN_NOT_FIND_SPEC = "光谱仪连接失败";

        public const String EMPTY_USER_NAME = "用户名不为空";

        public const String EMPTY_PASSWORD = "密码不为空";

        public const String SHORT_PASSWORD = "密码长度必须超过6位";

        public const String CONFIRMED_WRONG_PASSWORD = "确认密码错误";

        public const String SUCCESS_ADD_USER = "成功添加用户";

        public const String FAILURE_ADD_USER = "添加用户失败";

        public const String SUCCESS_DEL_USER = "成功删除用户";

        public const String FAILURE_DEL_USER = "删除用户失败";

        public const String NO_USER = "无该用户";

        public const String EMPTY_SEARCH_VALUE = "查询值为空";

        public const String DUP_SEARCH_FIELD = "查询字段重复";

        public const String NO_SELECTED_DATA = "无查询数据";

        public const String CONFIRMED_DELETE_DATA = "是否确认删除该条测试任务？";

        public const String CANNOT_DEL_CURRENT_USER = "不能删除当前用户";

        public const String SAVE_DATA_TO_EXCEL_PASS = "保存数据到Excel成功";
        //X超出范围提示
        public static String X_OVER_RANGE = "X的范围是" + MIN_X + "-" + MAX_X + "(mm)，请修改";

        //Y超出范围提示
        public static String Y_OVER_RANGE = "Y的范围是" + MIN_Y + "-" + MAX_Y + "(mm)，请修改";

        //测量角度超出范围提示
        public static String ANGLLE_OVER_RANGE = "测量角度是" + MIN_ANGLE + "-" + MAX_ANGLE + "度，请修改";

        //界面有没有输入的参数时提示
        public const String EMPTY_PARAM = "参数不能为空，请输入";

        //光谱仪像素个数
        public const int PIXELS_COUNT = 2048;

        //最大积分时间65秒
        public const int LARGEST_INTEGRATION_TIME = 65000000;

        //最小积分时间1毫秒
        public const int SMALLEST_INTEGRATION_TIME = 1000;

        //初始积分时间,100ms
        public const int DEFAULT_INTEGRATION_TIME = 100000;

        //最大平均次数，1000次
        public const int LARGEST_AVERAGE_TIMES = 1000;

        //最小平均次数，1次
        public const int SMALLEST_AVRAGE_TIMES = 1;

        //初始平均次数
        public const int DEFAULT_AVRAGE_TIMES = 1;

        //最大平滑度，20
        public const int LARGEST_BOXCAR_WIDTH = 20;

        //最小平滑度，1
        public const int SMALLEST_BOXCAR_WIDTH = 1;

        //初始平滑度
        public const int DEFAULT_BOXCAR_WIDTH = 1;

        //X轴最大
        public const int MAX_X = 160;

        //X轴最小
        public const int MIN_X = 0;

        //Y轴最大
        public const int MAX_Y = 160;

        //Y轴最小
        public const int MIN_Y = 0;

        //最大角度
        public const int MAX_ANGLE = 80;

        //最小角度
        public const int MIN_ANGLE = 15;

        //校准的X位置
        public const int AJUST_X_POS = 0;

        //校准的Y位置
        public const int AJUST_Y_POS = 0;

        //端口号
        public const int TCP_PORT = 2000;

        //默认照明光源名称
        public const String DEFAULT_LIGHT = "D65";

        //默认观察角名称
        public const String DEFAULT_ANGLE = "10-degree";

        //默认图中X轴最小值
        public const int DEFAULT_MIN_X_AXIS_VALUE = 350;

        //默认图中X轴最大值
        public const int DEFAULT_MAX_X_AXIS_VALUE = 800;

        //默认标准光谱图中Y轴最小值
        public const int DEFAULT_NORMAL_CHART_MIN_Y_AXIS_VALUE = 0;

        //默认标准光谱图中Y轴最大值
        public const int DEFAULT_NORMAL_CHART_MAX_Y_AXIS_VALUE = 65535;

        //默认反射光谱图中Y轴最小值
        public const int DEFAULT_RELECTIVITY_CHART_MIN_Y_AXIS_VALUE = 0;

        //默认反射光谱图中Y轴最大值
        public const int DEFAULT_RELECTIVITY_CHART_MAX_Y_AXIS_VALUE = 110;

        //原坐标原点在左上角，现移动到右上角，两点距离145mm，对于发送给PLC的值需要减去145
        public const int X_MOVE_OFFSET = 145;

        //在开始连续增加或者连续减少之前的计时
        public const Int64 INTERVEL_TO_CHANGE_100NS = 10000000;

        //连续增加或者减少时的时间间隔
        public const Int64 INVERVAL_OF_CHANGING_100NS = 450000;

        //画图是Y轴范围计算时，需要将最大最小值乘上的值
        public const double Y_AXIS_SCOPE_FACTOR = 1.05;

    }
}

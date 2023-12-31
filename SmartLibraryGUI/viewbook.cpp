#include "viewbook.h"
#include "ui_viewbook.h"

viewBook::viewBook(QWidget *parent) : QDialog(parent), ui(new Ui::viewBook)
{
    ui->setupUi(this);
    ui->isbn_Edit->setFocus();
    if(globalObj->isBleConnect())
    {
        ui->isbn_Edit->setPlaceholderText("请扫描或输入13位ISBN码");
        ui->scan_Btn->setEnabled(true);
        connect(globalObj, &GlobalProcess::bleRead, this, &viewBook::bleRead);
    }
}

viewBook::~viewBook()
{
    delete ui;
    if(globalObj->isBleConnect())
    {
        disconnect(globalObj, &GlobalProcess::bleRead, this, &viewBook::bleRead);
    }
}

void viewBook::bleRead(QString isbn)
{
    if(isbn.startsWith("978") && isbn.length() == 13)
    {
        if(isbn != ui->isbn_Edit->text().remove("-"))
        {
            ui->isbn_Edit->setText(isbn);
            ui->isbn_Edit->setInputMask("999-9-9999-9999-9");
            ui->isbn_Edit->setCursorPosition(17);
            on_search_Btn_clicked();
        }
    }
    else if(isbn == "over")
    {
        ui->Tip->setText("还书成功，3秒后自动退出");
        ui->isBorrowed_Label->setPixmap(tick);
        sql.returnBook(ui->isbn_Edit->text().remove("-"));
        QTimer::singleShot(3000, this, [&](){close();});
    }
    else
    {
        ui->Tip->setText("条码错误，请重新扫描");
    }
    ui->scan_Btn->setEnabled(true);
}

void viewBook::on_isbn_Edit_textChanged(const QString &str)
{
    if(str == "----")
    {
        ui->isbn_Edit->setInputMask("");
        ui->attitude_label->clear();
    }
    else if(str.length() == 1)
    {
        ui->isbn_Edit->setInputMask("999-9-9999-9999-9");
        ui->isbn_Edit->setCursorPosition(1);
        ui->attitude_label->setPixmap(wrong);
    }
    else if(str.length() != 17)
    {
        ui->search_Btn->setEnabled(false);
        ui->attitude_label->setPixmap(wrong);
    }
    else if(str.length() == 17)
    {
        ui->search_Btn->setEnabled(true);
        ui->attitude_label->setPixmap(TOOLS::loadImage(":/pic/right.png", QSize(40,40)));
    }
}

void viewBook::on_isbn_Edit_returnPressed()
{
    if(ui->isbn_Edit->text().length() == 17)
    {
        on_search_Btn_clicked();
    }
}

void viewBook::on_scan_Btn_clicked()
{
    ui->scan_Btn->setEnabled(false);
    globalObj->SocketWrite("scan");
}

void viewBook::on_search_Btn_clicked()
{
    ui->name_Edit->clear();
    ui->author_Edit->clear();
    ui->press_Edit->clear();
    ui->pressDate_Edit->clear();
    ui->pressPlace_Edit->clear();
    ui->price_Edit->clear();
    ui->pages_Edit->clear();
    ui->words_Edit->clear();
    ui->clc_Edit->clear();
    ui->bookDesc_Edit->clear();
    ui->shelfNum_Edit->clear();
    ui->isBorrowed_Label->clear();
    ui->Tip->clear();

    QString isbn = ui->isbn_Edit->text().remove("-");
    if(sql.exists(isbn))
    {
        QSqlRecord record = sql.getOneBookInfo("isbn", isbn);
        ui->name_Edit->setText(record.value("bookName").toString());
        ui->author_Edit->setText(record.value("author").toString());
        ui->press_Edit->setText(record.value("press").toString());
        ui->pressDate_Edit->setText(record.value("pressDate").toString());
        ui->pressPlace_Edit->setText(record.value("pressPlace").toString());
        ui->price_Edit->setText(record.value("price").toString());
        ui->pages_Edit->setText(record.value("pages").toString());
        ui->words_Edit->setText(record.value("words").toString());
        ui->clc_Edit->setText(record.value("clcName").toString());
        ui->bookDesc_Edit->setText(record.value("bookDesc").toString());
        ui->shelfNum_Edit->setText(record.value("shelfNumber").toString());
        ui->isBorrowed_Label->setPixmap(record.value("isBorrowed").toBool()?cross:tick);
        ui->borrow_return_Btn->setEnabled(true);
        if(record.value("isBorrowed").toBool())
        {
            ui->borrow_return_Btn->setText("还书");
        }
        else
        {
            ui->borrow_return_Btn->setText("借书");
        }
    }
    else
    {
        ui->Tip->setText("此书不在数据库中");
        ui->borrow_return_Btn->setEnabled(false);
    }
    ui->search_Btn->setEnabled(false);
}

void viewBook::on_borrow_return_Btn_clicked()
{
    if(ui->borrow_return_Btn->text() == "借书")
    {
        QString isbn = ui->isbn_Edit->text().remove("-");
        sql.borrowBook(isbn);
        ui->Tip->setText(QString("借书成功"));
        ui->isBorrowed_Label->setPixmap(cross);
        ui->borrow_return_Btn->setEnabled(false);
    }
    else
    {
        QString isbn = ui->isbn_Edit->text().remove("-");
        QSqlRecord record = sql.getOneBookInfo("isbn", isbn);
        if(globalObj->isBleConnect())
        {
            globalObj->SocketWrite(QString("带我去,%1").arg(record.value("shelfNumber").toString()));
            ui->Tip->setText(QString("小车正在启动，请前往%1号书架").arg(record.value("shelfNumber").toString()));
        }
        else
        {
            ui->Tip->setText(QString("蓝牙未连接，请前往%1号书架").arg(record.value("shelfNumber").toString()));
        }
        ui->borrow_return_Btn->setEnabled(false);
    }
}

void viewBook::on_quit_Btn_clicked()
{
    close();
}

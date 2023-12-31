#include "findbook.h"
#include "findbookcell.h"

#include "ui_findbook.h"

findBook::findBook(QWidget *parent) : QDialog(parent), ui(new Ui::findBook)
{
    ui->setupUi(this);
    ui->bookName_Edit->setFocus();
    if(globalObj->isBleConnect())
    {
        connect(globalObj, &GlobalProcess::bleRead, this, &findBook::bleRead);
    }
}

findBook::~findBook()
{
    if(globalObj->isBleConnect())
    {
        disconnect(globalObj, &GlobalProcess::bleRead, this, &findBook::bleRead);
    }
    delete ui;
}

void findBook::bleRead(QString isbn)
{
    if(isbn == "over")
    {
        QSqlRecord record = sql.getOneBookInfo("isbn", bookisbns.at(ui->listWidget->currentRow()));
        if(!record.value("isBorrowed").toBool())
        {
            if(QMessageBox::question(this, "找书成功", "是否借阅？") == QMessageBox::Yes)
            {
                QString isbn = bookisbns.at(ui->listWidget->currentRow());
                sql.borrowBook(isbn);
                ui->Tip->setText(QString("借书成功"));
                ui->isBorrowed_Label->setPixmap(cross);
                ui->find_Btn->setEnabled(false);
            }
        }
        else
        {
            ui->Tip->setText(QString("找书成功"));
        }
    }
}

void findBook::on_bookName_Edit_textChanged(const QString &str)
{
    ui->listWidget->clear();
    on_listWidget_itemClicked(nullptr);
    if(!str.isEmpty())
    {
        books = sql.getBooksInfoByName(str);
        if(!books.isEmpty())
        {
            foreach(QSqlRecord record, books)
            {
                QListWidgetItem *item = new QListWidgetItem();
                findbookcell *book = new findbookcell();
                book->bookName->setText(QString("《%1》").arg(record.value("bookName").toString()));
                book->press->setText(QString("%1  %2").arg(record.value("press").toString(), record.value("clcName").toString()));
                book->author->setText(record.value("author").toString());
                ui->listWidget->addItem(item);
                ui->listWidget->setItemWidget(item, book);
                bookisbns.append(record.value("isbn").toString());
            }
        }
    }
}

void findBook::on_listWidget_itemClicked(QListWidgetItem *item)
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

    if(item)
    {
        QString isbn = bookisbns.at(ui->listWidget->currentRow());
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
        ui->name_Edit->setCursorPosition(0);
        ui->find_Btn->setEnabled(true);
    }
    else
    {
        ui->find_Btn->setEnabled(false);
    }
}

void findBook::on_find_Btn_clicked()
{
    QString isbn = bookisbns.at(ui->listWidget->currentRow());
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
    ui->find_Btn->setEnabled(false);
}

void findBook::on_quit_Btn_clicked()
{
    close();
}

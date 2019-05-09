using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace EguiLab2
{
    class DialogVM
    {

        private Book _bookReference;
        public Book _currData { get; set; }

        public DialogVM(Book bookref = null)
        {
            _bookReference = bookref;
            _currData = bookref != null ? new Book(bookref.Author, bookref.Title, bookref.Year) : new Book();
        }

        public Book Result { get => _bookReference ?? _currData; }

        public ICommand OnOkButtonClick
        {
            get
            {
                if (_onOk == null)
                    _onOk = new RelayCommand(p => OnOkMethod(p));
                return _onOk;
            }
        }

        private void OnOkMethod(object p)
        {
            if(_bookReference != null)
            {
                _bookReference.Author = _currData.Author;
                _bookReference.Title = _currData.Title;
                _bookReference.Year = _currData.Year;
            }
            (p as Window).DialogResult = true;
            (p as Window).Close();
        }

        private void OnCancelMethod(object p)
        {
            (p as Window).DialogResult = false;
            (p as Window).Close();
        }

        RelayCommand _onOk;
        public ICommand OnCancel
        {
            get
            {
                if (_onCancel == null)
                    _onCancel = new RelayCommand(p => OnCancelMethod(p));
                return _onCancel;
            }
        }
        private ICommand _onCancel;
    }
}
